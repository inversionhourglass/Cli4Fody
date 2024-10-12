# Cli4Fody

[中文](https://github.com/inversionhourglass/Cli4Fody/blob/master/README.md) | English

Cli4Fody is a command-line tool used for managing Fody addins and modifying `FodyWeavers.xml` files. Through this tool, some Fody plugins can achieve completely non-intrusive MSIL modifications.

## Quick Start

Install Cli4Fody

> dotnet tool install -g Cli4Fody

Assume the current solution `MySolution.sln` has the following `FodyWeavers.xml`:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait />
</Weavers>
```

Execute the following command:

> fody-cli MySolution.sln --addin ConfigureAwait -a ContinueOnCapturedContext=true

The `FodyWeavers.xml` after executing the command:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait ContinueOnCapturedContext="true" />
</Weavers>
```

## Command Format

> fody-cli &lt;SolutionOrProjectPath&gt; [Options]

- `<solutionOrProjectPath>` The path to the solution file (*.sln) or project file (*.csproj).
- Options
    - `--share <project|solution>`

        Specifies the sharing setting for the `FodyWeavers.xml` file. The default is `project`, which creates a separate `FodyWeavers.xml` for each project. Setting it to `solution` will create only one `FodyWeavers.xml` file in the solution directory. This configuration is ineffective when `<solutionOrProjectPath>` is the path to a project file.
        
        Note: If all projects in the entire solution have the same `FodyWeavers.xml` content, you can generate a single `FodyWeavers.xml` in the solution directory. When a project detects that the `FodyWeavers.xml` file in its own directory is missing, it will automatically use the `FodyWeavers.xml` file from the solution directory. For more details, see: https://github.com/Fody/Home/blob/master/pages/configuration.md#a-file-in-the-solution-directory

    - `--order <ORDERS>`

        Customizes the plugin order. `<ORDERS>` is a series of plugin names separated by commas. You can use `_others` to represent unspecified other plugins. Suppose you have the following `FodyWeavers.xml`:
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <ConfigureAwait />
          <Rougamo />
          <NullGuard />
          <Pooling />
        </Weavers>
        ```
        After executing `fody-cli MyProject.csproj --order Rougamo,Pooling,_others` with Cli4Fody, you get the following `FodyWeavers.xml`:
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Rougamo />
          <Pooling />
          <ConfigureAwait />
          <NullGuard />
        </Weavers>
        ```

    - `--addin <ADDIN>`

        Configures the plugin `<ADDIN>`. `<ADDIN>` is the name of the plugin (without the `Fody` suffix). If the `FodyWeavers.xml` already exists and the `<ADDIN>` node does not exist, it appends the `<ADDIN>` node to the end. If the `FodyWeavers.xml` file does not exist, it generates a `FodyWeavers.xml` file and adds the `<ADDIN>` node.
        
        Example: `fody-cli MyProject.csproj --addin Rougamo`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Rougamo />
        </Weavers>
        ```

    - `-pv, --package-version`

        Plugin version, a sub-option of `--addin`. When this parameter is specified, Cli4Fody will add the NuGet package of the current plugin to each project, with the version being the current parameter value. This parameter can be used to resolve issues where indirect dependencies on MSBuild tasks by Fody plugins are ineffective. By adding a direct NuGet dependency to each project, this issue can be mitigated. It's important to note that to speed up the execution of Cli4Fody, NuGet packages are installed using the `--no-restore` option.

    - `-m, --mode <overwrite|default>`

        Operation mode, a sub-option of `--addin`. The default is `overwrite`, which creates or overwrites an existing `--addin` configuration node. `default` means to set it as the default configuration; if the `--addin` configuration node already exists, no changes are made. Note that `-m, --mode` must be specified after `--addin` as a sub-option of `--addin`.
        
        Correct usage: `fody-cli MySolution.sln --addin Rougamo -m default`
        
        Incorrect usage: `fody-cli MySolution.sln -m default --addin Rougamo`

    - `-n, --node <NODE>`

        Adds a child node to the plugin, a sub-option of `--addin`. For multi-level nodes, you can use `:` to express multiple levels, such as `Items:Item` indicating the `Item` node under the `Items` node under the current plugin configuration node. Specifying the same node path multiple times indicates appending.
        
        Example: `fody-cli MySolution.sln --addin Pooling -n Items:Item -n Items:Item`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Pooling>
            <Items>
              <Item />
              <Item />
            </Items>
          </Pooling>
        </Weavers>
        ```

    - `-a, --attribute <ATTRIBUTE>`

        Adds attributes to plugins or nodes, a sub-option of both `--addin` and `--node`.

        Example: `fody-cli MySolution.sln --addin Pooling -a enabled=true -n Items:Item -a "pattern=method(* StringBuilder.Clear(..))" -n Items:Item -a stateless=Random`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Pooling enabled="true">
            <Items>
              <Item pattern="method(* StringBuilder.Clear(..))" />
              <Item stateless="Random" />
            </Items>
          </Pooling>
        </Weavers>
        ```

    - `-v, --value <VALUE>`

        Sets the value of a node for plugins or nodes, a sub-option of both `--addin` and `--node`.

        Example: `fody-cli MySolution.sln --addin Pooling -n Inspects:Inspect -v "execution(* *(..))"`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Pooling enabled="true">
            <Inspects>
              <Inspect>execution(* *(..))</Inspect>
            </Inspects>
          </Pooling>
        </Weavers>
        ```

## Non-Intrusive Code Weaving Case

[Pooling](https://github.com/inversionhourglass/Pooling) uses Cli4Fody to achieve [non-intrusive object pooling operation replacement](https://github.com/inversionhourglass/Pooling/blob/master/README_en.md#zero-intrusion-pooling).
