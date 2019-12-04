// <Snippet3>
open System
open System.Runtime.InteropServices

[<LiteralAttribute>]
let STD_OUTPUT_HANDLE = -11;
[<LiteralAttribute>]
let TMPF_TRUETYPE = 4;
[<LiteralAttribute>]
let LF_FACESIZE = 32;
let INVALID_HANDLE_VALUE = IntPtr(-1);

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type (*internal*) COORD =
    val mutable X: int16
    val mutable Y: int16

    internal new(x : int16, y : int16) = { X = x; Y = y }
 
[<Struct>]
[<StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)>]
type (*internal*) CONSOLE_FONT_INFO_EX =
    val mutable cbSize : uint32
    val mutable nFont : uint32
    val mutable dwFontSize : COORD
    val mutable FontFamily : int
    val mutable FontWeight : int
    [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)>]
    val mutable FaceName : string

[<DllImport("kernel32.dll", SetLastError = true)>]
extern IntPtr internal GetStdHandle(int nStdHandle)

[<DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)>]
extern bool internal GetCurrentConsoleFontEx(
    IntPtr consoleOutput, 
    bool maximumWindow,
    CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx)
       
[<DllImport("kernel32.dll", SetLastError = true)>]
extern bool internal SetCurrentConsoleFontEx(
    IntPtr consoleOutput, 
    bool maximumWindow,
    CONSOLE_FONT_INFO_EX consoleCurrentFontEx)

[<EntryPoint>]
let main argv =
    let fontName = "Lucida Console";

    let getStdOutHandle = 
        GetStdHandle(STD_OUTPUT_HANDLE)
        |> fun x -> match x with
        | INVALID_HANDLE_VALUE -> Error ("Invalid handle")
        | _ -> Ok x
    
    let getCurrentConsoleFontInfo hnd =
        let mutable info = CONSOLE_FONT_INFO_EX()
        info.cbSize <- uint32 (Marshal.SizeOf(info));
        // First determine whether there's already a TrueType font.
        if GetCurrentConsoleFontEx(hnd, false, info)
        then Ok info 
        else Error "Failed call to GetCurrentConsoleFontEx"

    let isTrueTypeFont (info : CONSOLE_FONT_INFO_EX) =
        if ((info.FontFamily) &&& TMPF_TRUETYPE) = TMPF_TRUETYPE
        then Ok info 
        else (Error "The console already is using a TrueType font.")

    let setConsoleFontToLucidaConsole (consoleOutput: IntPtr) (info : CONSOLE_FONT_INFO_EX) =
        let mutable newInfo = CONSOLE_FONT_INFO_EX()
        newInfo.cbSize <- uint32 (Marshal.SizeOf(newInfo))          
        newInfo.FontFamily <- TMPF_TRUETYPE
        newInfo.FaceName <- fontName
        // Get some settings from current font.
        newInfo.dwFontSize <- COORD(info.dwFontSize.X, info.dwFontSize.Y)
        newInfo.FontWeight <- info.FontWeight
        SetCurrentConsoleFontEx(consoleOutput, false, newInfo) |> ignore


    getStdOutHandle 
    |> Result.map (fun hnd -> 
        Result.bind getCurrentConsoleFontInfo
        >> Result.bind isTrueTypeFont
        >> Result.bind ((setConsoleFontToLucidaConsole hnd) >> fun _ -> Ok ())
        >> Result.map (fun _ -> Console.WriteLine("The console is now using a TrueType font."))
        >> Result.mapError (fun message -> Console.WriteLine(message))
    )
    |> ignore
    
    0
   
// </Snippet3>
