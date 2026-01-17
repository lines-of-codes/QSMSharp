# QSMSharp

A Quick Server Manager for Minecraft, ironically written in C# and not Java.

This software plans to have basic server management and configuration,
mods/plugins management with modpack installation, and Java installation management.

This software stemmed from the fact that I love Prism Launcher's mod management
and wished for an open-source Minecraft server management with the same level of
capabilities.

## Code Quality

Issues as reported by DeepSource.

| Project     | Issues                                                                                                                                                                                                                   |
|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| QSM.Core    | [![DeepSource](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Core.svg/?label=active+issues&show_trend=true&token=_e2ck9mkRm_mUZmNguCMxHoY)](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Core/) |
| QSM.Web     | [![DeepSource](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Web.svg/?label=active+issues&show_trend=true&token=On0sEKH7_f_XchVoKuzj_7n9)](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Web/)   |
| QSM.Windows | [![DeepSource](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Windows.svg/?label=active+issues&show_trend=true&token=W7gVvSwdgM8yv1DO2OdwCG0h)](https://app.deepsource.com/gh/lines-of-codes/QSMSharp:QSM.Windows/) |

## Projects

In the solution, There are three projects including:

- QSM.Core: Common classes shared between projects
- QSM.Web: Cross-platform web version of the management software
- QSM.Windows: Original Windows-only version, More complete.

## License

The project is licensed under the GNU GPLv3.

The Starlight UI translations for the Thai language (`qsm-website/src/content/i18n/th.json`) is MIT licensed as it is the same file I added into the Starlight PR to officially add the translations into the framework.
