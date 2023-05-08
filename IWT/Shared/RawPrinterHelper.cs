using System;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IWT.Shared
{

    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "RAW Document";
            // Win7
            di.pDataType = "RAW";

            // Win8+
            // di.pDataType = "XPS_PASS";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendFileToPrinter( string szFileName, string szPrinterName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            fs.Close();
            fs.Dispose();
            fs = null;
            return bSuccess;
        }
        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }

        //public class RawPrinterHelper
        //{
        //    // Structure and API declarions:
        //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //    private class DOCINFOA
        //    {
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pDocName;
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pOutputFile;
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pDataType;
        //    }

        //    #region dll Wrappers
        //    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        //    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool ClosePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        //    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool EndDocPrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool StartPagePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool EndPagePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        //    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        //    public static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int size);

        //    #endregion dll Wrappers

        //    #region Methods
        //    /// <summary>
        //    /// This function gets the pdf file name.
        //    /// This function opens the pdf file, gets all its bytes & send them to print.
        //    /// </summary>
        //    /// <param name="szPrinterName">Printer Name</param>
        //    /// <param name="szFileName">Pdf File Name</param>
        //    /// <returns>true on success, false on failure</returns>
        //    public static bool SendFileToPrinter(string pdfFileName, string printerName)
        //    {
        //        try
        //        {
        //            #region Get Connected Printer Name
        //            PrintDocument pd = new PrintDocument();
        //            StringBuilder dp = new StringBuilder(256);
        //            int size = dp.Capacity;
        //            if (string.IsNullOrEmpty(printerName))
        //            {
        //                if (GetDefaultPrinter(dp, ref size))
        //                {
        //                    pd.PrinterSettings.PrinterName = dp.ToString().Trim();
        //                }
        //            }
        //            else
        //            {
        //                pd.PrinterSettings.PrinterName = printerName;
        //            }

        //            #endregion Get Connected Printer Name

        //            // Open the PDF file.
        //            FileStream fs = new FileStream(pdfFileName, FileMode.Open, FileAccess.Read);
        //            // Create a BinaryReader on the file.
        //            BinaryReader br = new BinaryReader(fs);
        //            Byte[] bytes = new Byte[fs.Length];
        //            bool success = false;
        //            // Unmanaged pointer.
        //            IntPtr ptrUnmanagedBytes = new IntPtr(0);
        //            int nLength = Convert.ToInt32(fs.Length);
        //            // Read contents of the file into the array.
        //            bytes = br.ReadBytes(nLength);
        //            // Allocate some unmanaged memory for those bytes.
        //            ptrUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
        //            // Copy the managed byte array into the unmanaged array.
        //            Marshal.Copy(bytes, 0, ptrUnmanagedBytes, nLength);
        //            // Send the unmanaged bytes to the printer.
        //            success = SendBytesToPrinter(pd.PrinterSettings.PrinterName, ptrUnmanagedBytes, nLength);
        //            // Free the unmanaged memory that you allocated earlier.
        //            Marshal.FreeCoTaskMem(ptrUnmanagedBytes);
        //            return success;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }

        //    /// <summary>
        //    /// This function gets the printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
        //    /// </summary>
        //    /// <param name="szPrinterName">Printer Name</param>
        //    /// <param name="pBytes">No. of bytes in the pdf file</param>
        //    /// <param name="dwCount">Word count</param>
        //    /// <returns>True on success, false on failure</returns>
        //    private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        //    {
        //        try
        //        {
        //            Int32 dwError = 0, dwWritten = 0;
        //            IntPtr hPrinter = new IntPtr(0);
        //            DOCINFOA di = new DOCINFOA();
        //            bool success = false; // Assume failure unless you specifically succeed.

        //            di.pDocName = "Document";
        //            di.pDataType = "RAW";

        //            // Open the printer.
        //            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
        //            {
        //                // Start a document.
        //                if (StartDocPrinter(hPrinter, 1, di))
        //                {
        //                    // Start a page.
        //                    if (StartPagePrinter(hPrinter))
        //                    {
        //                        // Write the bytes.
        //                        success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
        //                        EndPagePrinter(hPrinter);
        //                    }
        //                    EndDocPrinter(hPrinter);
        //                }
        //                ClosePrinter(hPrinter);
        //            }

        //            // If print did not succeed, GetLastError may give more information about the failure.
        //            if (success == false)
        //            {
        //                dwError = Marshal.GetLastWin32Error();
        //            }
        //            return success;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }
        //    #endregion Methods
        //}

        //public class RawPrinterHelper
        //{
        //    //// Structure and API declarions:
        //    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //    //private class DOCINFOA
        //    //{
        //    //    [MarshalAs(UnmanagedType.LPStr)]
        //    //    public string pDocName;
        //    //    [MarshalAs(UnmanagedType.LPStr)]
        //    //    public string pOutputFile;
        //    //    [MarshalAs(UnmanagedType.LPStr)]
        //    //    public string pDataType;
        //    //}

        //    //#region dll Wrappers
        //    //[DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        //    //[DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool ClosePrinter(IntPtr hPrinter);

        //    //[DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        //    //[DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool EndDocPrinter(IntPtr hPrinter);

        //    //[DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool StartPagePrinter(IntPtr hPrinter);

        //    //[DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool EndPagePrinter(IntPtr hPrinter);

        //    //[DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    //private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        //    //[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        //    //public static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int size);

        //    //#endregion dll Wrappers

        //    //#region Methods
        //    ///// <summary>
        //    ///// This function gets the pdf file name.
        //    ///// This function opens the pdf file, gets all its bytes & send them to print.
        //    ///// </summary>
        //    ///// <param name="szPrinterName">Printer Name</param>
        //    ///// <param name="szFileName">Pdf File Name</param>
        //    ///// <returns>true on success, false on failure</returns>
        //    //public static bool SendFileToPrinter(string pdfFileName, bool IsLandscape, string printerName)
        //    //{
        //    //    try
        //    //    {
        //    //        #region Get Connected Printer Name
        //    //        PrintDocument pd = new PrintDocument();
        //    //        pd.PrinterSettings.DefaultPageSettings.Landscape = IsLandscape;
        //    //        StringBuilder dp = new StringBuilder(256);
        //    //        int size = dp.Capacity;
        //    //        if (string.IsNullOrEmpty(printerName))
        //    //        {
        //    //            if (GetDefaultPrinter(dp, ref size))
        //    //            {
        //    //                pd.PrinterSettings.PrinterName = dp.ToString().Trim();
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            pd.PrinterSettings.PrinterName = printerName;
        //    //        }

        //    //        #endregion Get Connected Printer Name

        //    //        // Open the PDF file.
        //    //        FileStream fs = new FileStream(pdfFileName, FileMode.Open, FileAccess.Read);
        //    //        // Create a BinaryReader on the file.
        //    //        BinaryReader br = new BinaryReader(fs);
        //    //        Byte[] bytes = new Byte[fs.Length];
        //    //        bool success = false;
        //    //        // Unmanaged pointer.
        //    //        IntPtr ptrUnmanagedBytes = new IntPtr(0);
        //    //        int nLength = Convert.ToInt32(fs.Length);
        //    //        // Read contents of the file into the array.
        //    //        bytes = br.ReadBytes(nLength);
        //    //        // Allocate some unmanaged memory for those bytes.
        //    //        ptrUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
        //    //        // Copy the managed byte array into the unmanaged array.
        //    //        Marshal.Copy(bytes, 0, ptrUnmanagedBytes, nLength);
        //    //        // Send the unmanaged bytes to the printer.
        //    //        success = SendBytesToPrinter(pd.PrinterSettings.PrinterName, ptrUnmanagedBytes, nLength);
        //    //        // Free the unmanaged memory that you allocated earlier.
        //    //        Marshal.FreeCoTaskMem(ptrUnmanagedBytes);
        //    //        return success;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        throw new Exception(ex.Message);
        //    //    }
        //    //}

        //    ///// <summary>
        //    ///// This function gets the printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
        //    ///// </summary>
        //    ///// <param name="szPrinterName">Printer Name</param>
        //    ///// <param name="pBytes">No. of bytes in the pdf file</param>
        //    ///// <param name="dwCount">Word count</param>
        //    ///// <returns>True on success, false on failure</returns>
        //    //private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        //    //{
        //    //    try
        //    //    {
        //    //        Int32 dwError = 0, dwWritten = 0;
        //    //        IntPtr hPrinter = new IntPtr(0);
        //    //        DOCINFOA di = new DOCINFOA();
        //    //        bool success = false; // Assume failure unless you specifically succeed.

        //    //        di.pDocName = "Document";
        //    //        //di.pDataType = "RAW";

        //    //        // Open the printer.
        //    //        if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
        //    //        {
        //    //            // Start a document.
        //    //            if (StartDocPrinter(hPrinter, 1, di))
        //    //            {
        //    //                // Start a page.
        //    //                if (StartPagePrinter(hPrinter))
        //    //                {
        //    //                    // Write the bytes.
        //    //                    success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
        //    //                    EndPagePrinter(hPrinter);
        //    //                }
        //    //                EndDocPrinter(hPrinter);
        //    //            }
        //    //            ClosePrinter(hPrinter);
        //    //        }

        //    //        // If print did not succeed, GetLastError may give more information about the failure.
        //    //        if (success == false)
        //    //        {
        //    //            dwError = Marshal.GetLastWin32Error();
        //    //        }
        //    //        return success;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        throw new Exception(ex.Message);
        //    //    }
        //    //}
        //    //#endregion Methods

        //    // Structure and API declarions:
        //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //    private class DOCINFOA
        //    {
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pDocName;
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pOutputFile;
        //        [MarshalAs(UnmanagedType.LPStr)]
        //        public string pDataType;
        //    }

        //    #region dll Wrappers
        //    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        //    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool ClosePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        //    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool EndDocPrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool StartPagePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool EndPagePrinter(IntPtr hPrinter);

        //    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        //    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        //    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        //    public static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int size);

        //    #endregion dll Wrappers

        //    #region Methods
        //    /// <summary>
        //    /// This function gets the pdf file name.
        //    /// This function opens the pdf file, gets all its bytes & send them to print.
        //    /// </summary>
        //    /// <param name="szPrinterName">Printer Name</param>
        //    /// <param name="szFileName">Pdf File Name</param>
        //    /// <returns>true on success, false on failure</returns>
        //    public static bool SendFileToPrinter(string pdfFileName)
        //    {
        //        try
        //        {
        //            #region Get Connected Printer Name
        //            PrintDocument pd = new PrintDocument();
        //            StringBuilder dp = new StringBuilder(256);
        //            int size = dp.Capacity;
        //            if (GetDefaultPrinter(dp, ref size))
        //            {
        //                pd.PrinterSettings.PrinterName = dp.ToString().Trim();
        //            }
        //            #endregion Get Connected Printer Name

        //            // Open the PDF file.
        //            FileStream fs = new FileStream(pdfFileName, FileMode.Open);
        //            // Create a BinaryReader on the file.
        //            BinaryReader br = new BinaryReader(fs);
        //            Byte[] bytes = new Byte[fs.Length];
        //            bool success = false;
        //            // Unmanaged pointer.
        //            IntPtr ptrUnmanagedBytes = new IntPtr(0);
        //            int nLength = Convert.ToInt32(fs.Length);
        //            // Read contents of the file into the array.
        //            bytes = br.ReadBytes(nLength);
        //            // Allocate some unmanaged memory for those bytes.
        //            ptrUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
        //            // Copy the managed byte array into the unmanaged array.
        //            Marshal.Copy(bytes, 0, ptrUnmanagedBytes, nLength);
        //            // Send the unmanaged bytes to the printer.
        //            success = SendBytesToPrinter(pd.PrinterSettings.PrinterName, ptrUnmanagedBytes, nLength);
        //            // Free the unmanaged memory that you allocated earlier.
        //            Marshal.FreeCoTaskMem(ptrUnmanagedBytes);
        //            return success;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }

        //    /// <summary>
        //    /// This function gets the printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
        //    /// </summary>
        //    /// <param name="szPrinterName">Printer Name</param>
        //    /// <param name="pBytes">No. of bytes in the pdf file</param>
        //    /// <param name="dwCount">Word count</param>
        //    /// <returns>True on success, false on failure</returns>
        //    private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        //    {
        //        try
        //        {
        //            Int32 dwError = 0, dwWritten = 0;
        //            IntPtr hPrinter = new IntPtr(0);
        //            DOCINFOA di = new DOCINFOA();
        //            bool success = false; // Assume failure unless you specifically succeed.

        //            di.pDocName = "PDF Document";
        //            di.pDataType = "RAW";

        //            // Open the printer.
        //            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
        //            {
        //                // Start a document.
        //                if (StartDocPrinter(hPrinter, 1, di))
        //                {
        //                    // Start a page.
        //                    if (StartPagePrinter(hPrinter))
        //                    {
        //                        // Write the bytes.
        //                        success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
        //                        EndPagePrinter(hPrinter);
        //                    }
        //                    EndDocPrinter(hPrinter);
        //                }
        //                ClosePrinter(hPrinter);
        //            }

        //            // If print did not succeed, GetLastError may give more information about the failure.
        //            if (success == false)
        //            {
        //                dwError = Marshal.GetLastWin32Error();
        //            }
        //            return success;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }
        //    #endregion Methods
        //}
    }
}

