using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Client.Excel
{
    [ComVisible(true)]
    [Guid("A5FCD106-BDA8-4D7C-820C-3C46A4D0ADBD")]
    public interface IPlugInConnection
    {
        void Connect([MarshalAs(UnmanagedType.IDispatch)] object application);

        bool IsPluginActive();

        void Disconnect();

        void AddDataButtonClicked();

        void ExploreButtonClicked();
    }
}
