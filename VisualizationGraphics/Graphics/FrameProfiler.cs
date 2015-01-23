using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine.Graphics
{
    public class FrameProfiler : DisposableResource
    {
        private Queue<ProfileFrameSection> framesAwaitingResults = new Queue<ProfileFrameSection>();
        private Stack<ProfileSection> liveSections = new Stack<ProfileSection>();
        private Queue<ProfileFrameSection> cachedFrameSections = new Queue<ProfileFrameSection>();
        private Queue<ProfileSection> cachedSections = new Queue<ProfileSection>();
        private List<ProfileResult> tempResults = new List<ProfileResult>();
        private List<ProfileResult[]> latestResults = new List<ProfileResult[]>();
        private object resultsLock = new object();
        public const int FrameCountForAverage = 30;
        private ProfileFrameSection currentFrameSection;
        private bool profilerEnabled;

        public bool Enabled { get; set; }

        public ProfileResult[] FrameProfile
        {
            get
            {
                lock (this.resultsLock)
                {
                    if (this.latestResults.Count == 0)
                        return null;
                    ProfileResult[] result = new ProfileResult[this.latestResults[0].Length];
                    Array.Copy(this.latestResults[0], result, this.latestResults[0].Length);
                    return result;
                }
            }
        }

        public ProfileResult[] AverageFrameProfile
        {
            get
            {
                lock (this.resultsLock)
                {
                    if (this.latestResults.Count == 0)
                        return null;
                    ProfileResult[] result = this.FrameProfile;
                    int latestResultUseCount = 1;
                    for (int i = 1; i < this.latestResults.Count; ++i)
                    {
                        bool flag = true;
                        if (this.latestResults[i].Length == result.Length)
                        {
                            for (int j = 0; j < result.Length; ++j)
                            {
                                if (!this.latestResults[i][j].Name.Equals(result[j].Name))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        else
                            flag = false;
                        if (flag)
                            ++latestResultUseCount;
                        else
                            break;
                    }
                    for (int i = 0; i < result.Length; ++i)
                    {
                        float duration = result[i].Duration.HasValue ? result[i].Duration.Value : 0.0f;
                        for (int j = 1; j < latestResultUseCount; ++j)
                        {
                            float? latestDuration = this.latestResults[j][i].Duration;
                            duration += latestDuration.HasValue ? latestDuration.Value : 0.0f;
                        }
                        result[i] = new ProfileResult(result[i].Name, new float?(duration / (float)latestResultUseCount), result[i].Level);
                    }
                    return result;
                }
            }
        }

        public FrameProfiler(bool enabled)
        {
            this.Enabled = this.profilerEnabled = enabled;
        }

        internal bool BeginFrame(Renderer renderer, int frame)
        {
            this.liveSections.Clear();
            if (this.Enabled != this.profilerEnabled)
            {
                if (this.profilerEnabled)
                    this.QueryProfileResults();
                this.profilerEnabled = this.Enabled;
            }
            if (!this.profilerEnabled)
            {
                this.latestResults.Clear();
                return false;
            }
            else
            {
                string str = "Frame";
                ProfileFrameSection profileFrameSection;
                if (this.cachedFrameSections.Count > 0)
                {
                    profileFrameSection = this.cachedFrameSections.Dequeue();
                    profileFrameSection.Reset(str, null);
                }
                else
                    profileFrameSection = ProfileFrameSection.Create(str, renderer);
                this.liveSections.Push(profileFrameSection);
                this.currentFrameSection = profileFrameSection;
                return profileFrameSection.Begin();
            }
        }

        internal bool EndFrame()
        {
            if (!this.profilerEnabled)
                return false;
            return this.EndFrame(false);
        }

        internal bool DiscardFrame()
        {
            if (!this.profilerEnabled)
                return false;
            return this.EndFrame(true);
        }

        private bool EndFrame(bool discard)
        {
            if (!this.profilerEnabled)
                return false;
            if (this.liveSections.Count != 1)
            {
                this.QueryProfileResults();
                this.liveSections.Clear();
                return false;
            }
            else
            {
                this.liveSections.Pop();
                bool flag = this.currentFrameSection.End();
                this.currentFrameSection.Discard = discard;
                this.framesAwaitingResults.Enqueue(this.currentFrameSection);
                this.currentFrameSection = null;
                this.QueryProfileResults();
                return flag;
            }
        }

        public bool BeginSection(string name)
        {
            if (!this.profilerEnabled || this.liveSections.Count == 0)
                return false;
            ProfileSection section;
            if (this.cachedSections.Count > 0)
            {
                section = this.cachedSections.Dequeue();
                section.Reset(name, this.currentFrameSection);
            }
            else
                section = ProfileSection.Create(name, this.currentFrameSection);
            this.liveSections.Peek().AddSubSection(section);
            section.Begin();
            this.liveSections.Push(section);
            return true;
        }

        public bool EndSection()
        {
            if (!this.profilerEnabled || this.liveSections.Count <= 1)
                return false;
            this.liveSections.Pop().End();
            return true;
        }

        private void QueryProfileResults()
        {
            while (this.framesAwaitingResults.Count > 0 && this.framesAwaitingResults.Peek().IsReady)
            {
                this.tempResults.Clear();
                ProfileFrameSection profileFrameSection = this.framesAwaitingResults.Dequeue();
                bool flag = !profileFrameSection.Discard;
                this.QueryResults(profileFrameSection, 0);
                this.cachedFrameSections.Enqueue(profileFrameSection);
                if (flag)
                {
                    lock (this.resultsLock)
                    {
                        this.latestResults.Insert(0, this.tempResults.ToArray());
                        if (this.latestResults.Count > 30)
                            this.latestResults.RemoveAt(this.latestResults.Count - 1);
                    }
                }
            }
        }

        private void QueryResults(ProfileSection section, int level)
        {
            this.tempResults.Add(new ProfileResult(section.Name, section.Duration, level));
            for (int index = 0; index < section.Sections.Count; ++index)
                this.QueryResults(section.Sections[index], level + 1);
            if (level == 0)
                return;
            this.cachedSections.Enqueue(section);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.ReleaseGraphicsResources();
        }

        private void DisposeSubSections(ProfileSection section)
        {
            for (int index = 0; index < section.Sections.Count; ++index)
                this.DisposeSubSections(section.Sections[index]);
            section.Dispose();
        }

        private void ReleaseGraphicsResources()
        {
            foreach (ProfileSection section in this.framesAwaitingResults)
                this.DisposeSubSections(section);
            this.framesAwaitingResults.Clear();
            foreach (DisposableResource disposableResource in this.cachedFrameSections)
                disposableResource.Dispose();
            this.cachedFrameSections.Clear();
            foreach (DisposableResource disposableResource in this.cachedSections)
                disposableResource.Dispose();
            this.cachedSections.Clear();
            foreach (DisposableResource disposableResource in this.liveSections)
                disposableResource.Dispose();
            this.liveSections.Clear();
        }
    }
}
