using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

[assembly: AssemblyVersion("1.0.0.0")]

// ================= Interfaces COM =================
[ComImport, Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IShellItem
{
    void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
    void GetParent(out IShellItem ppsi);
    void GetDisplayName(uint sigdnName, out IntPtr ppszName);
    void GetAttributes(uint mask, out uint attribs);
    void Compare(IShellItem psi, uint hint, out int order);
}

[ComImport, Guid("b63ea76d-1f85-456f-a19c-48159efa858b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IShellItemArray
{
    void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
    void GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);
    void GetPropertyDescriptionList(IntPtr keyType, ref Guid riid, out IntPtr ppv);
    void GetAttributes(int attribFlags, uint mask, out uint attribs);
    void GetCount(out uint count);
    void GetItemAt(uint index, out IShellItem item);
    void EnumItems(out IntPtr ppenumShellItems);
}

[ComImport, Guid("a08ce4d0-fa25-44ab-b57c-c7b1c323e0b9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IExplorerCommand
{
    [PreserveSig] int GetTitle(IShellItemArray items, [MarshalAs(UnmanagedType.LPWStr)] out string name);
    [PreserveSig] int GetIcon(IShellItemArray items, [MarshalAs(UnmanagedType.LPWStr)] out string icon);
    [PreserveSig] int GetToolTip(IShellItemArray items, [MarshalAs(UnmanagedType.LPWStr)] out string tip);
    [PreserveSig] int GetCanonicalName(out Guid guid);
    [PreserveSig] int GetState(IShellItemArray items, [MarshalAs(UnmanagedType.Bool)] bool okToBeSlow, out uint state);
    [PreserveSig] int Invoke(IShellItemArray items, IntPtr bindCtx);
    [PreserveSig] int GetFlags(out uint flags);
    [PreserveSig] int EnumSubCommands(out IntPtr enumCmds);
}

[ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IFileOpenDialog
{
    [PreserveSig] int Show(IntPtr parent);
    void SetFileTypes(uint c, IntPtr rgSpec);
    void SetFileTypeIndex(uint i);
    void GetFileTypeIndex(out uint i);
    void Advise(IntPtr pfde, out uint cookie);
    void Unadvise(uint cookie);
    void SetOptions(uint fos);
    void GetOptions(out uint fos);
    void SetDefaultFolder(IShellItem psi);
    void SetFolder(IShellItem psi);
    void GetFolder(out IShellItem ppsi);
    void GetCurrentSelection(out IShellItem ppsi);
    void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
    void GetFileName(out IntPtr name);
    void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
    void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
    void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
    void GetResult(out IShellItem ppsi);
    void AddPlace(IShellItem psi, int fdap);
    void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string ext);
    void Close(int hr);
    void SetClientGuid(ref Guid guid);
    void ClearClientData();
    void SetFilter(IntPtr filter);
    void GetResults(out IntPtr ppenum);
    void GetSelectedItems(out IntPtr ppsai);
}

[ComImport, Guid("dc1c5a9c-e88a-4dde-a5a1-60f82a20aef7")]
class FileOpenDialogRCW { }

// ================= Extensi\u00f3n del men\u00fa: "Eliminar enlace" solo visible sobre enlaces =================
[ComVisible(true), Guid("7d9f5b8a-3c21-4e6b-9a1d-5f0c2b7e4a11"), ClassInterface(ClassInterfaceType.None)]
public class DeleteLinkCommand : IExplorerCommand
{
    const int E_NOTIMPL = unchecked((int)0x80004001);

    static List<string> Paths(IShellItemArray items)
    {
        List<string> list = new List<string>();
        if (items == null) return list;
        uint n; items.GetCount(out n);
        for (uint i = 0; i < n; i++)
        {
            IShellItem it; items.GetItemAt(i, out it);
            IntPtr p; it.GetDisplayName(0x80058000, out p);   // SIGDN_FILESYSPATH
            list.Add(Marshal.PtrToStringUni(p));
            Marshal.FreeCoTaskMem(p);
            Marshal.ReleaseComObject(it);
        }
        return list;
    }

    public int GetTitle(IShellItemArray items, out string name) { Program.EnsureLang(); name = Program.MenuDeleteText(); return 0; }
    public int GetIcon(IShellItemArray items, out string icon) { icon = Program.IconPath("icon_delete.ico"); return 0; }
    public int GetToolTip(IShellItemArray items, out string tip) { tip = null; return E_NOTIMPL; }
    public int GetCanonicalName(out Guid guid) { guid = Guid.Empty; return E_NOTIMPL; }
    public int GetFlags(out uint flags) { flags = 0; return 0; }
    public int EnumSubCommands(out IntPtr enumCmds) { enumCmds = IntPtr.Zero; return E_NOTIMPL; }

    public int GetState(IShellItemArray items, bool okToBeSlow, out uint state)
    {
        state = 2;   // ECS_HIDDEN
        try
        {
            List<string> ps = Paths(items);
            if (ps.Count == 0) return 0;
            foreach (string p in ps) if (!Program.IsLink(p)) return 0;
            state = 0;   // ECS_ENABLED
        }
        catch { }
        return 0;
    }

    public int Invoke(IShellItemArray items, IntPtr bindCtx)
    {
        try
        {
            foreach (string p in Paths(items))
                Process.Start(Program.SelfPath, "delete \"" + p + "\"");
        }
        catch { }
        return 0;
    }
}

static class Program
{
    // ---------- Idiomas: title, ask, junction, symDir, symFile, hard, ok, cancel, pick, done, notLink, confirm, deleted, delTitle, menuCreate, menuDelete, installed, uninstalled
    static Dictionary<string, string[]> Langs = new Dictionary<string, string[]>();
    static string[] T;

    static void InitLangs()
    {
        Langs["es"] = new string[] { "Crear enlace simb\u00f3lico", "\u00bfQu\u00e9 tipo de enlace quieres crear?", "Uni\u00f3n (mklink /J)", "Enlace simb\u00f3lico (mklink /D)", "Enlace simb\u00f3lico (mklink)", "Enlace duro (mklink /H)", "Aceptar", "Cancelar", "Elige la carpeta donde crear el enlace", "Enlace creado:", "El elemento seleccionado no es un enlace.", "\u00bfEliminar este enlace?\n(El original NO se borrar\u00e1)", "Enlace eliminado.", "Eliminar enlace", "Crear enlace simb\u00f3lico", "Eliminar enlace", "Instalado correctamente.", "Desinstalado correctamente." };
        Langs["en"] = new string[] { "Create symbolic link", "Which type of link do you want to create?", "Junction (mklink /J)", "Symbolic link (mklink /D)", "Symbolic link (mklink)", "Hard link (mklink /H)", "OK", "Cancel", "Choose the folder where the link will be created", "Link created:", "The selected item is not a link.", "Delete this link?\n(The original will NOT be deleted)", "Link deleted.", "Delete link", "Create symbolic link", "Delete link", "Installed successfully.", "Uninstalled successfully." };
        Langs["fr"] = new string[] { "Cr\u00e9er un lien symbolique", "Quel type de lien voulez-vous cr\u00e9er ?", "Jonction (mklink /J)", "Lien symbolique (mklink /D)", "Lien symbolique (mklink)", "Lien physique (mklink /H)", "OK", "Annuler", "Choisissez le dossier o\u00f9 cr\u00e9er le lien", "Lien cr\u00e9\u00e9 :", "L'\u00e9l\u00e9ment s\u00e9lectionn\u00e9 n'est pas un lien.", "Supprimer ce lien ?\n(L'original ne sera PAS supprim\u00e9)", "Lien supprim\u00e9.", "Supprimer le lien", "Cr\u00e9er un lien symbolique", "Supprimer le lien", "Install\u00e9 avec succ\u00e8s.", "D\u00e9sinstall\u00e9 avec succ\u00e8s." };
        Langs["de"] = new string[] { "Symbolischen Link erstellen", "Welchen Link-Typ m\u00f6chten Sie erstellen?", "Junction (mklink /J)", "Symbolischer Link (mklink /D)", "Symbolischer Link (mklink)", "Hardlink (mklink /H)", "OK", "Abbrechen", "Ordner f\u00fcr den Link ausw\u00e4hlen", "Link erstellt:", "Das ausgew\u00e4hlte Element ist kein Link.", "Diesen Link l\u00f6schen?\n(Das Original wird NICHT gel\u00f6scht)", "Link gel\u00f6scht.", "Link l\u00f6schen", "Symbolischen Link erstellen", "Link l\u00f6schen", "Erfolgreich installiert.", "Erfolgreich deinstalliert." };
        Langs["pt"] = new string[] { "Criar link simb\u00f3lico", "Que tipo de link deseja criar?", "Jun\u00e7\u00e3o (mklink /J)", "Link simb\u00f3lico (mklink /D)", "Link simb\u00f3lico (mklink)", "Link f\u00edsico (mklink /H)", "OK", "Cancelar", "Escolha a pasta onde criar o link", "Link criado:", "O item selecionado n\u00e3o \u00e9 um link.", "Excluir este link?\n(O original N\u00c3O ser\u00e1 exclu\u00eddo)", "Link exclu\u00eddo.", "Excluir link", "Criar link simb\u00f3lico", "Excluir link", "Instalado com sucesso.", "Desinstalado com sucesso." };
        Langs["it"] = new string[] { "Crea collegamento simbolico", "Quale tipo di collegamento vuoi creare?", "Junction (mklink /J)", "Collegamento simbolico (mklink /D)", "Collegamento simbolico (mklink)", "Collegamento reale (mklink /H)", "OK", "Annulla", "Scegli la cartella in cui creare il collegamento", "Collegamento creato:", "L'elemento selezionato non \u00e8 un collegamento.", "Eliminare questo collegamento?\n(L'originale NON verr\u00e0 eliminato)", "Collegamento eliminato.", "Elimina collegamento", "Crea collegamento simbolico", "Elimina collegamento", "Installato con successo.", "Disinstallato con successo." };
    }

    const int S_TITLE = 0, S_ASK = 1, S_JUNCTION = 2, S_SYMDIR = 3, S_SYMFILE = 4, S_HARD = 5, S_OK = 6, S_CANCEL = 7,
              S_PICK = 8, S_DONE = 9, S_NOTLINK = 10, S_CONFIRM = 11, S_DELETED = 12, S_DELTITLE = 13,
              S_MENUCREATE = 14, S_MENUDELETE = 15, S_INSTALLED = 16, S_UNINSTALLED = 17;

    // ---------- Win32 ----------
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool CreateSymbolicLink(string link, string target, int flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool CreateHardLink(string link, string existing, IntPtr reserved);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr sec, uint disp, uint flags, IntPtr tmpl);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetFileInformationByHandle(SafeFileHandle h, out BY_HANDLE_FILE_INFORMATION info);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr FindFirstFile(string name, out WIN32_FIND_DATA data);
    [DllImport("kernel32.dll")]
    static extern bool FindClose(IntPtr h);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    [StructLayout(LayoutKind.Sequential)]
    struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime, LastAccessTime, LastWriteTime;
        public uint VolumeSerialNumber, FileSizeHigh, FileSizeLow, NumberOfLinks, FileIndexHigh, FileIndexLow;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime, ftLastAccessTime, ftLastWriteTime;
        public uint nFileSizeHigh, nFileSizeLow, dwReserved0, dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternateFileName;
    }

    const uint TAG_SYMLINK = 0xA000000C, TAG_MOUNT_POINT = 0xA0000003;
    const string CLSID_DELETE = "{7d9f5b8a-3c21-4e6b-9a1d-5f0c2b7e4a11}";

    internal static string SelfPath { get { return Assembly.GetExecutingAssembly().Location; } }
    internal static string IconPath(string file) { return Path.Combine(Path.GetDirectoryName(SelfPath), file) + ",0"; }
    internal static string MenuDeleteText() { return T[S_MENUDELETE]; }

    internal static void EnsureLang()
    {
        if (T != null) return;
        InitLangs();
        string lg = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        T = Langs.ContainsKey(lg) ? Langs[lg] : Langs["en"];
    }

    // ---------- Main ----------
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        EnsureLang();
        if (args.Length == 0) return;
        try
        {
            switch (args[0])
            {
                case "--install": Install(true); break;
                case "--install-simple": Install(false); break;
                case "--uninstall": Uninstall(); break;
                case "create": if (args.Length > 1) Create(args[1]); break;
                case "delete": if (args.Length > 1) Delete(args[1]); break;
                case "create-do": if (args.Length > 4) CreateDo(args[1], args[2], args[3], args[4] == "1"); break;
            }
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }

    // Error siempre delante de todo (MB_ICONERROR | MB_TOPMOST | MB_SETFOREGROUND)
    static void ShowError(string m) { MessageBoxW(IntPtr.Zero, m, T[S_TITLE], 0x10 | 0x40000 | 0x10000); }
    static string Q(string s) { return "\"" + s + "\""; }
    static string Trim(string p) { return p.Length > 3 ? p.TrimEnd('\\') : p; }

    // ---------- Instalaci\u00f3n ----------
    static readonly string[] Roots = { "*", "Directory" };

    static void AddVerb(string root, string key, string label, string action, string icon)
    {
        using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + root + @"\shell\" + key))
        {
            k.SetValue("MUIVerb", label);
            k.SetValue("Icon", icon);
            using (RegistryKey c = k.CreateSubKey("command"))
                c.SetValue("", Q(SelfPath) + " " + action + " \"%1\"");
        }
    }

    static void Install(bool smart)
    {
        Uninstall();
        string dir = Path.GetDirectoryName(SelfPath);
        foreach (string r in Roots)
        {
            AddVerb(r, "SymlinkCtx_Create", T[S_MENUCREATE], "create", Path.Combine(dir, "icon_create.ico"));
            if (smart)
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + r + @"\shell\SymlinkCtx_Delete"))
                    k.SetValue("ExplorerCommandHandler", CLSID_DELETE);
            }
            else AddVerb(r, "SymlinkCtx_Delete", T[S_MENUDELETE], "delete", Path.Combine(dir, "icon_delete.ico"));
        }
        if (smart)
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\Classes\CLSID\" + CLSID_DELETE))
            {
                k.SetValue("", "Symlink in Context Menu - Delete link");
                using (RegistryKey s = k.CreateSubKey("InprocServer32"))
                {
                    s.SetValue("", "mscoree.dll");
                    s.SetValue("ThreadingModel", "Both");
                    s.SetValue("Class", "DeleteLinkCommand");
                    s.SetValue("Assembly", Assembly.GetExecutingAssembly().FullName);
                    s.SetValue("RuntimeVersion", "v4.0.30319");
                    s.SetValue("CodeBase", new Uri(SelfPath).AbsoluteUri);
                }
            }
        }
    }

    static void Uninstall()
    {
        foreach (string r in Roots)
            foreach (string k in new string[] { "SymlinkCtx_Create", "SymlinkCtx_Delete" })
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\" + r + @"\shell\" + k, false);
        Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\CLSID\" + CLSID_DELETE, false);
    }

    // ---------- Di\u00e1logos ----------
    static Form TopOwner()
    {
        Form o = new Form();
        o.ShowInTaskbar = false; o.TopMost = true; o.FormBorderStyle = FormBorderStyle.None;
        o.StartPosition = FormStartPosition.Manual; o.Location = new Point(-32000, -32000); o.Size = new Size(1, 1);
        o.Show(); o.Activate();
        return o;
    }

    static string AskType(bool isDir)
    {
        string[] codes = isDir ? new string[] { "J", "S" } : new string[] { "S", "H" };
        string[] labels = isDir ? new string[] { T[S_JUNCTION], T[S_SYMDIR] } : new string[] { T[S_SYMFILE], T[S_HARD] };
        using (Form f = new Form())
        {
            f.Text = T[S_TITLE];
            f.StartPosition = FormStartPosition.CenterScreen;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false; f.MinimizeBox = false; f.TopMost = true;
            f.ClientSize = new Size(380, 150);
            try { f.Icon = Icon.ExtractAssociatedIcon(SelfPath); } catch { }
            f.Shown += delegate { f.Activate(); };
            Label lbl = new Label(); lbl.Text = T[S_ASK]; lbl.AutoSize = true; lbl.Location = new Point(15, 15);
            f.Controls.Add(lbl);
            RadioButton[] rb = new RadioButton[2];
            for (int i = 0; i < 2; i++)
            {
                rb[i] = new RadioButton(); rb[i].Text = labels[i]; rb[i].AutoSize = true;
                rb[i].Location = new Point(25, 45 + 30 * i); rb[i].Checked = (i == 0);
                f.Controls.Add(rb[i]);
            }
            Button ok = new Button(); ok.Text = T[S_OK]; ok.DialogResult = DialogResult.OK; ok.Location = new Point(185, 110);
            Button ca = new Button(); ca.Text = T[S_CANCEL]; ca.DialogResult = DialogResult.Cancel; ca.Location = new Point(270, 110);
            f.Controls.Add(ok); f.Controls.Add(ca); f.AcceptButton = ok; f.CancelButton = ca;
            if (f.ShowDialog() != DialogResult.OK) return null;
            return rb[0].Checked ? codes[0] : codes[1];
        }
    }

    // Selector de carpetas moderno de Windows (el mismo del Explorador)
    static string PickFolder()
    {
        using (Form owner = TopOwner())
        {
            IFileOpenDialog d = (IFileOpenDialog)new FileOpenDialogRCW();
            uint opts; d.GetOptions(out opts);
            d.SetOptions(opts | 0x20 | 0x40 | 0x800);   // PICKFOLDERS | FORCEFILESYSTEM | PATHMUSTEXIST
            d.SetTitle(T[S_PICK]);
            if (d.Show(owner.Handle) != 0) return null;
            IShellItem item; d.GetResult(out item);
            IntPtr p; item.GetDisplayName(0x80058000, out p);
            string s = Marshal.PtrToStringUni(p);
            Marshal.FreeCoTaskMem(p);
            return s;
        }
    }

    // ---------- Crear ----------
    static void Create(string path)
    {
        path = Trim(path);
        bool isDir = Directory.Exists(path);
        if (!isDir && !File.Exists(path)) { ShowError(path); return; }

        string type = AskType(isDir);
        if (type == null) return;

        string destDir = PickFolder();
        if (destDir == null) return;

        string name = Path.GetFileName(path);
        if (name.Length == 0) name = path.Substring(0, 1) + "_drive";
        string baseName = isDir ? name : Path.GetFileNameWithoutExtension(name);
        string ext = isDir ? "" : Path.GetExtension(name);
        string link = Path.Combine(destDir, name);
        int n = 0;
        while (File.Exists(link) || Directory.Exists(link))
        {
            n++;
            link = Path.Combine(destDir, baseName + (n == 1 ? " - link" : " - link (" + n + ")") + ext);
        }

        int code;
        string err = MakeLink(type, link, path, isDir, out code);
        if (err == null) return;   // \u00e9xito: sin popup

        if (code == 1314 && !IsAdmin())   // falta privilegio: relanzar elevado (UAC)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(SelfPath,
                    "create-do " + type + " " + Q(path) + " " + Q(link) + " " + (isDir ? "1" : "0"));
                psi.Verb = "runas"; psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Win32Exception) { /* UAC cancelado */ }
        }
        else ShowError(err);
    }

    static void CreateDo(string type, string target, string link, bool isDir)
    {
        int code;
        string err = MakeLink(type, link, target, isDir, out code);
        if (err != null) ShowError(err);
    }

    static bool IsAdmin()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    // devuelve null si OK, o el mensaje de error
    static string MakeLink(string type, string link, string target, bool isDir, out int code)
    {
        code = 0;
        if (type == "J")
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c mklink /J " + Q(link) + " " + Q(target));
            psi.CreateNoWindow = true; psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true; psi.RedirectStandardError = true;
            using (Process p = Process.Start(psi))
            {
                string o = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode == 0) return null;
                code = p.ExitCode;
                return o.Trim();
            }
        }
        bool ok;
        if (type == "S")
        {
            int flags = (isDir ? 1 : 0) | 2;  // 2 = permitir sin administrador (Modo desarrollador)
            ok = CreateSymbolicLink(link, target, flags);
            if (!ok && Marshal.GetLastWin32Error() == 87) ok = CreateSymbolicLink(link, target, isDir ? 1 : 0);
        }
        else ok = CreateHardLink(link, target, IntPtr.Zero);

        if (ok) return null;
        code = Marshal.GetLastWin32Error();
        return new Win32Exception(code).Message;
    }

    // ---------- Detecci\u00f3n / eliminaci\u00f3n ----------
    static uint LinkCount(string path)
    {
        using (SafeFileHandle h = CreateFile(path, 0, 7, IntPtr.Zero, 3, 0, IntPtr.Zero))
        {
            if (h.IsInvalid) return 1;
            BY_HANDLE_FILE_INFORMATION i;
            return GetFileInformationByHandle(h, out i) ? i.NumberOfLinks : 1;
        }
    }

    static bool IsReparseLink(string path)
    {
        WIN32_FIND_DATA d;
        IntPtr h = FindFirstFile(path, out d);
        if (h == new IntPtr(-1)) return false;
        FindClose(h);
        return (d.dwFileAttributes & 0x400) != 0 && (d.dwReserved0 == TAG_SYMLINK || d.dwReserved0 == TAG_MOUNT_POINT);
    }

    internal static bool IsLink(string path)
    {
        path = Trim(path);
        bool isDir = Directory.Exists(path);
        return IsReparseLink(path) || (!isDir && File.Exists(path) && LinkCount(path) > 1);
    }

    static void Delete(string path)
    {
        path = Trim(path);
        if (!IsLink(path)) { ShowError(T[S_NOTLINK]); return; }
        if (Directory.Exists(path)) Directory.Delete(path, false);   // solo borra el enlace, nunca el contenido del destino
        else File.Delete(path);
    }
}
