# Remap OMEN key to Home

Remap the OMEN key to Home for HP OMEN laptops.

The Home key on HP OMEN laptops is replaced by a special key that launches the Command Center application. It is not recognized as a keyboard key by Windows. 

It is possible to change the function of this key back to Home, by putting a small program in place of the Command Center link, which simulates the Home key to the operating system.

## Guide

First, the driver "HP System Event Utility" must be installed.

Download [send-home.exe](https://github.com/jingyu9575/remap-omen-key/releases) and put it in a fixed location.

Run `regedit`, navigate to `HKEY_CURRENT_USER\Software\HP\HP System Event\Bezel Button\8613`, and change the value of `ApplicationPath` to the path of the downloaded program.

## UAC Issues

By default, the auto-started HP Message Service does not run with the Administrator privilege, so when an elevated application is active, the remapped OMEN key may not work.

To resolve this issue, run `taskschd.msc` and create a basic task:

* Name: HP Message Service
* Trigger: When I log on
* Action: Start a program
* Program: `"C:\Program Files (x86)\HP\HP System Event\HPMSGSVC.exe"`

Double-click the created task and enable "Run with Highest Privileges".

The default unprivileged auto-start can be disabled in the Start-up tab of Task Manager.
