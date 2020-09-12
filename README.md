# Remap OMEN key to Home
Remap the OMEN key to Home for HP OMEN laptops.

This branch creates a custom client that listens to the driver and sends the HOME key event directly. It is supposed to be more responsive than the registry approach. 

First, the driver "HP System Event Utility" (with HPMSGSVC.exe) must be installed.

Download [custom-client.exe](https://github.com/jingyu9575/remap-omen-key/releases). It is a signed executable with [UIAccess](https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/user-account-control-allow-uiaccess-applications-to-prompt-for-elevation-without-using-the-secure-desktop) privilege to resolve UAC issues. The certificate must be installed before starting the program. Right-click the file, select the certificate tab, and install it into the *Trusted People* store. Also, it must be put inside special directories such as *Program Files (x86)* to work with elevated windows.
