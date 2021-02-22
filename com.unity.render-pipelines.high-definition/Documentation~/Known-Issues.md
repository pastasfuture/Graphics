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

If you use the wizard, the local package should be in `ROOT/LocalPackages/com.unity.render-pipelines.high-definition-config` and you `ROOT/Packages/manifest.json` should already target this local package.

1. Create a folder `ROOT/Assets/Packages/com.unity.render-pipelines.high-definition-config`
1. Copy mostly all contents from `ROOT/LocalPackages/com.unity.render-pipelines.high-definition-config` to `ROOT/Assets/Packages/com.unity.render-pipelines.high-definition-config`
    1. Only keep `package.json` and `package.json.meta`
1. Create a symlink from `ROOT/Assets/Packages/com.unity.render-pipelines.high-definition-config/Runtime/ShaderConfig.cs.hlsl` to `ROOT/LocalPackages/com.unity.render-pipelines.high-definition-config/Runtime/ShaderConfig.cs.hlsl`
    1. On windows, with an administrator shell: `mklink LocalPackages\com.unity.render-pipelines.high-definition-config\Runtime\ShaderConfig.cs.hlsl Assets\Packages\com.unity.render-pipelines.high-definition-config\Runtime\ShaderConfig.cs.hlsl /H`
    1. On unix: `ln -s Assets/Packages/com.unity.render-pipelines.high-definition-config/Runtime/ShaderConfig.cs.hlsl LocalPackages/com.unity.render-pipelines.high-definition-config/Runtime/ShaderConfig.cs.hlsl`
1. Make sure all files in `ROOT/Assets/Packages/com.unity.render-pipelines.high-definition-config` are in collab.
