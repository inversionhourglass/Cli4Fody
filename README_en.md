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

> fody-cli MySolution.sln --addin ConfigureAwait -a ContinueOnCapturedContext=false --addin Rougamo

The `FodyWeavers.xml` after executing the command:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait ContinueOnCapturedContext="false" />
  <Rougamo />
</Weavers>
```

## Command Format

> fody-cli &lt;SolutionOrProjectPath&gt; [Options]

- `<solutionOrProjectPath>` The path to the solution file (*.sln) or project file (*.csproj).
- Options
    - `--include-fody <VERSION>`

        Add Fody NuGet dependency, with `<VERSION>` being the version number of Fody.

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

        Plugin version, a sub-option of `--addin`. When this parameter is specified, Cli4Fody will add the NuGet package dependency of the current plugin to each project, with the version being the current parameter value. This parameter can be used to resolve issues where indirect dependencies on MSBuild tasks by Fody plugins are ineffective. By adding a direct NuGet dependency to each project, this issue can be mitigated. It's important to note that to speed up the execution of Cli4Fody, NuGet packages are installed using the `--no-restore` option.

    - `-m, --mode <overwrite|default>`

        Operation mode, a sub-option of `--addin`. The default is `overwrite`, which creates or overwrites an existing `--addin` configuration node. `default` means to set it as the default configuration; if the `--addin` configuration node already exists, no changes are made. Note that `-m, --mode` must be specified after `--addin` as a sub-option of `--addin`.
        
        Incorrect usage: `fody-cli MySolution.sln -m default --addin Rougamo`
        
        Correct usage: `fody-cli MySolution.sln --addin Rougamo -m default`

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

## Usage Scenarios

Cli4Fody has two primary use cases:

1. **Direct Dependency on Fody Plugins**: 
   You may have noticed that when using Fody plugins, your project must directly depend on them. Indirect dependencies don’t work. For example, in the dependency chain [Project A] → [Project B] → Rougamo.Fody, Project A can use the types defined in Rougamo.Fody, but the corresponding MSIL (Microsoft Intermediate Language) won’t be modified unless Project A directly depends on Rougamo.Fody. This is because Fody modifies the MSIL through tasks inserted into the MSBuild pipeline after compilation, and these tasks are included in the Fody NuGet package under the `build` directory. In an indirect dependency scenario, the build configuration won’t be transmitted automatically, so each project needs to directly depend on the Fody plugin.

   Since Fody plugin types can be used indirectly, it’s easy to forget to directly add the dependency, causing the plugin to not function properly. With Cli4Fody, you can resolve this issue by adding the direct dependency to every project in the solution. The following command will add a direct dependency on Rougamo.Fody to every project in the solution and create or update the `FodyWeavers.xml` file.
   
   ```
   fody-cli MySolution.sln --addin Rougamo -pv 4.0.4
   ```

2. **Configuration-Only Plugins**:
   Some Fody plugins, like ConfigureAwait.Fody and Pooling.Fody, can achieve their objectives through configuration alone. For these types of plugins, you can use Cli4Fody to directly configure them.

   Example command:
   ```shell
   fody-cli MySolution.sln \
             --addin ConfigureAwait -pv 3.3.2 -a ContinueOnCapturedContext=false \
             --addin Pooling -pv 0.1.0 \
                 -n Inspects:Inspect -v "execution(* *..*Service.*(..))" \
                 -n Items:Item -a stateless=Random
   ```

### Recommended Practices

Cli4Fody is well-suited for continuous integration (CI). You can easily configure it in an automated build process, ensuring both Fody plugin direct dependencies and non-invasive plugin configurations are applied consistently. For example, refer to the [DockerSample](https://github.com/inversionhourglass/Cli4Fody/tree/master/samples/DockerSample) project, where a Dockerfile uses Cli4Fody to ensure a direct dependency on Rougamo.Fody and add ConfigureAwait configuration.

```docker
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ConsoleApp/ConsoleApp.csproj", "ConsoleApp/"]
RUN dotnet restore "./ConsoleApp/ConsoleApp.csproj"

COPY . .

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install -g Cli4Fody
RUN fody-cli DockerSample.sln --addin Rougamo -pv 4.0.4 --addin ConfigureAwait -pv 3.3.2  -a ContinueOnCapturedContext=false

RUN dotnet restore "./ConsoleApp/ConsoleApp.csproj"

RUN dotnet publish "./ConsoleApp/ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]
```

## Pooling Zero-Intrusive Practice

[Pooling](https://github.com/inversionhourglass/Pooling) uses Cli4Fody to achieve [non-intrusive object pooling operation replacement](https://github.com/inversionhourglass/Pooling/blob/master/README_en.md#zero-intrusion-pooling).
