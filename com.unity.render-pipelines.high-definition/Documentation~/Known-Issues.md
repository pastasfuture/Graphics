# Known issues

This page contains information on known about issues you may encounter while using HDRP. Each entry describes the issue and then details the steps to follow in order to resolve the issue.

## Material array size

If you upgrade your HDRP Project to a later version, you may encounter an error message similar to:

```
Property (_Env2DCaptureForward) exceeds previous array size (48 vs 6). Cap to previous size.

UnityEditor.EditorApplication:Internal_CallGlobalEventHandler()
```

To fix this issue, restart the Unity editor.

## Collab and Config

If you installed locally the HDRP config package and use Collaborate as your versionning software, then the local package is not included in the versioning.

### Scripted workaround:
We provide a script to do the workaround, it requires:
1. Python 3.9
1. The right to create symbolinc links on windows. (You can execute as an administrator the script or give your user the right to create symbolic links)

To create and version the HDRP config package:
1. Install the config package from the Wizard
1. Run the utility script bundled in hdrp: `Packages/com.unity.render-pipelines.high-definition-config/Documentation~/tools/local_package_collab.py -p <PATH_TO_YOUR_UNITY_PROJECT>`
1. In Unity, check in all modified and added files

To download from Collaborate or sync from collab the project:
1. Clone or sync from Collaborate the project
1. Run the utility script bundled in hdrp: `Packages/com.unity.render-pipelines.high-definition-config/Documentation~/tools/local_package_collab.py -p <PATH_TO_YOUR_UNITY_PROJECT>`

### Manual workaround
We will need to move files in order to get them versionned by Collaborate.

So, you will need to go from this folder structure (only important files are shown for readibility):
* Root
    * LocalPackages
        * com.unity.render-pipelines.high-definition-config
            * package.json
            * Runtime
                * ShaderConfig.cs
                * ShaderConfig.cs.hlsl
                * Unity.RenderPipelines.HighDefinition.Config.Runtime.asmdef
            * Tests
            * Documentation~
    * Assets

To:
* Root
    * LocalPackages
        * com.unity.render-pipelines.high-definition-config
            * package.json
            * Runtime
                * ShaderConfig.cs.hlsl (hard symlink to Assets/Packages/com.unity.render-pipelines.high-definition-config/ShaderConfig.cs.hlsl)
    * Assets
        * Packages
            * com.unity.render-pipelines.high-definition-config
                * Runtime
                    * ShaderConfig.cs
                    * ShaderConfig.cs.hlsl
                    * Unity.RenderPipelines.HighDefinition.Config.Runtime.asmdef
                * Tests

Note: _Hidden files and folders will be ignored by Collaborate, so they won't be versionned and be lost. But only non essential files are concerned, like the mostly empty Documentation folder of the config package._

Note: _On windows you can use the `mklink /H <link> <target>` command to create the symbolic link. This will require the Create Symbolic Link right for your user, you can also use an Administrator shell._
Note: _On linux and OSX, you can use the `ln -s <target> <link>` command to create thr symbolic link._
