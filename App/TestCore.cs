using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class TestCore
    {
        public TestState testState;

        public void ExploreButtonClicked()
        {
            try
            {
                testState = new TestState();
                if (!testState.IsInitialized)
                {
                    if (!testState.Initialize())
                    {
                        return;
                    }
                }
                testState.Explore(true);
            }
            catch (Exception exception3)
            {
                VisualizationTraceSource.Current.Fail(exception3);
                this.HandleException(testState, exception3);
            }
        }

        private void HandleException(TestState workbookStateAtFault, Exception ex)
        {
            try
            {
                if (workbookStateAtFault != null)
                {
                    this.ReleaseWorkbookState(workbookStateAtFault, false);
                    return;
                }
            }
            catch (Exception exception)
            {
                VisualizationTraceSource.Current.Fail(exception);
            }
        }

        private void ReleaseWorkbookState(TestState workbookState, bool saveBeforeRelease)
        {
            try
            {
                workbookState.UnInitialize(saveBeforeRelease);
            }
            catch (Exception exception)
            {
                VisualizationTraceSource.Current.Fail(exception);
            }
        }
    }
}
