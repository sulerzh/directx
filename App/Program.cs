using Microsoft.Data.Visualization.Utilities;
using Microsoft.Data.Visualization.VisualizationCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConsoleApplication1
{
    class Program
    {
        private static TestState testState;
        public static TestState TestState
        {
            get
            {
                return testState;
            }
            set
            {
                if (testState != null)
                {
                    testState.OnHostWindowClosed -= new EventHandler(onHostWindowClosed);
                    testState.OnHostWindowOpened -= new EventHandler(onHostWindowOpened);
                }
                testState = value;
                if (testState != null)
                {
                    testState.OnHostWindowClosed += new EventHandler(onHostWindowClosed);
                    testState.OnHostWindowOpened += new EventHandler(onHostWindowOpened);
                }
            }
        }

        private static void onHostWindowOpened(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void onHostWindowClosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            //string cultureInfoName = CultureInfo.CurrentUICulture.Name;zh-CN
            Properties.Resources.Culture = new CultureInfo("zh-CN");
            TestCore core = new TestCore();
            core.ExploreButtonClicked();
            TestState = core.testState;
        }
    }
}
