# Windows Symlink Context Menu

Adds the ability to create and delete symbolic links directly from the Windows context menu (right-click). This open-source project removes the friction of having to open the command prompt to run `mklink`, bringing this powerful feature to a fully visual environment.

## What is a symbolic link?

A symbolic link (or *symlink*) is a type of file that points to another file or directory. Unlike classic Windows shortcuts (`.lnk`), the operating system, terminal, and third-party applications treat the symbolic link exactly as if it were the original file or folder.

If a program is configured to read data from the `C:\Data` folder, you can create a symbolic link with that name pointing to `D:\Backup\Data`. The program will work perfectly, believing it is reading from the C drive, without needing to be reconfigured.

## Comparison: Windows vs. Linux

In **Linux/UNIX** based systems, symbolic links are a fundamental and ubiquitous feature. They are easily created using the `ln -s` command, and the file system (like ext4) manages them natively. Their use is common even for basic users when organizing configurations or libraries.

In **Windows**, although the NTFS file system has supported symbolic links since Windows Vista, the native tool to create them has been restricted to the command line (`mklink` in CMD or `New-Item` in PowerShell), usually requiring administrator privileges. This has caused their use to be residual compared to the less efficient shortcuts. This project seeks to match the user experience to that of Linux desktop environments, integrating inode/link management directly into the graphical user interface (GUI).

## Main Features

* **2-click creation:** Select any file or folder, right-click, and generate its symbolic link.

* **Safe deletion:** Delete the link without the risk of deleting the original files from the hard drive.

* **Zero commands:** Forget about remembering terminal syntax and *flags* (like `/D`, `/J` or `/H`).

* **Zero performance impact:** Pure integration via the Windows Registry, with no applications running in the background and no extra RAM or CPU consumption.

## Installation

1. Download the latest release file from the **Releases** tab.

2. Unzip the `.zip` file.

3. Double-click the `Install_SymlinkMenu.reg` file (or run the included `.bat` script).

4. Accept the User Account Control (UAC) prompt to add the keys to the Windows Registry.

5. *(Optional)* If the changes don't appear immediately, restart Windows Explorer from the Task Manager.

## Usage

1. Navigate to the original file or directory.

2. Right-click and select **"Copy path for link"**.

3. Go to the destination folder, right-click on the background, and choose **"Paste as symbolic link"**.

4. To safely delete it, select the link, right-click, and choose **"Delete symbolic link"**.

## Contributing

Contributions are very welcome! If you want to propose improvements, optimize the code for the new Windows 11 context menu, or fix bugs:

1. *Fork* the repository.

2. Create your feature branch (`git checkout -b feature/NewFeature`).

3. *Commit* your changes (`git commit -m 'Add new feature'`).

4. *Push* to the branch (`git push origin feature/NewFeature`).

5. Open a *Pull Request*.

## License

Distributed under the MIT License. See the `LICENSE` file for more information.