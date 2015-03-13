using System.Runtime.InteropServices;

namespace Microsoft.Data.Visualization.Client.Excel
{
    public interface IPlugInConnectionExternalAddin
    {
        void ConnectExternalAddin([MarshalAs(UnmanagedType.IDispatch)] object application, [MarshalAs(UnmanagedType.IDispatch)] object ribbonUI);

        bool IsPluginActiveExternalAddin();

        void DisconnectExternalAddin();

        void AddDataButtonClickedExternalAddin([MarshalAs(UnmanagedType.IDispatch)] object ribbonControl);

        void ExploreButtonClickedExternalAddin([MarshalAs(UnmanagedType.IDispatch)] object ribbonControl);

        bool GetEnabledExternalAddin([MarshalAs(UnmanagedType.IDispatch)] object ribbonControl);
    }
}
