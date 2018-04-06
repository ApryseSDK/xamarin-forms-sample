# xamarin-forms-sample

**Targeting PDFTron for Xamarin SDK v6.8+**

This sample shows how to use PDFTron for Xamarin in Xamarin.Forms project via `PageRenderer`. You can run this sample on Android, iOS and UWP. It shows a simple `PDFViewCtrl` with annotation functionality.

To run this sample, you will first need to request a demo license key [here](https://www.pdftron.com).

Then, put your demo key in `/Common/Key.cs`.

To run the Android or iOS project, install the corresponding NuGet packages.

To run the UWP project, please install the `PDFNetUWPApps.vsix` file inside the `/lib` folder from the download package.

From the UWP download package, copy `/Samples/PDFViewCtrlTools_VS2015` folder to inside `ContentPage` folder. i.e. you will have `/ContentPage/PDFViewCtrlTools_VS2015` folder.