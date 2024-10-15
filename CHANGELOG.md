# Cli4Fody

中文 | [English](https://github.com/inversionhourglass/Cli4Fody/blob/master/README_en.md)

Cli4Fody是一个命令行工具，用于管理Fody插件和修改`FodyWeavers.xml`文件。通过该工具，可以让部分Fody插件实现完全零侵入式的MSIL修改。

## 快速开始

安装Cli4Fody

> dotnet tool install -g Cli4Fody

假设当前解决方案`MySolution.sln`下有如下`FodyWeavers.xml`：

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait />
</Weavers>
```

执行如下命令：

> fody-cli MySolution.sln --addin ConfigureAwait -a ContinueOnCapturedContext=false --addin Rougamo

执行命令后的`FodyWeavers.xml`：

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait ContinueOnCapturedContext="false" />
  <Rougamo />
</Weavers>
```

## 命令格式

> fody-cli &lt;SolutionOrProjectPath&gt; [Options]

- `<solutionOrProjectPath>` 解决方案文件(*.sln)路径或项目文件(*.csproj)路径
- Options
    - `--include-fody <VERSION>`

      增加Fody NuGet依赖，`<VERSION>`为Fody的版本号。

    - `--share <project|solution>`

        `FodyWeavers.xml`文件共享设置，默认`project`，为每个项目创建一个`FodyWeavers.xml`，设置为`solution`时仅在解决方案目录创建`FodyWeavers.xml`文件，当`<solutionOrProjectPath>`为项目文件路径时，该配置无效。
        
        注：如果整个解决方案所有的项目的`FodyWeavers.xml`文件内容是一样的，可以仅在解决方案目录下生成一个`FodyWeavers.xml`，当项目检测到当前项目目录下的`FodyWeavers.xml`文件缺省时，会自动使用解决方案目录下的`FodyWeavers.xml`文件，详见：https://github.com/Fody/Home/blob/master/pages/configuration.md#a-file-in-the-solution-directory

    - `--order <ORDERS>`
        
        自定义插件顺序，`<ORDERS>`为一系列插件名称，插件名称用`,`分隔，可以使用`_others`指代未指定的其他插件。假设有如下`FodyWeavers.xml`文件：
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <ConfigureAwait />
          <Rougamo />
          <NullGuard />
          <Pooling />
        </Weavers>
        ```
        使用Cli4Fody执行`fody-cli MyProject.csproj --order Rougamo,Pooling,_others_` 后得到如下`FodyWeavers.xml`：
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Rougamo />
          <Pooling />
          <ConfigureAwait />
          <NullGuard />
        </Weavers>
        ```

    - `--addin <ADDIN>`

        配置插件`ADDIN`，`<ADDIN>`为插件名称（不要`Fody`后缀），如果`FodyWeavers.xml`已存在且`ADDIN`节点不存在则追加到`ADDIN`节点到最后。如果`FodyWeavers.xml`文件不存在，则生成`FodyWeavers.xml`文件并增加`ADDIN`节点。
        
        例：`fody-cli MyProject.csproj --addin Rougamo`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Rougamo />
        </Weavers>
        ```

    - `-pv, --package-version`

        插件版本，`--addin`的子配置项。当指定该参数后，Cli4Fody会为每个项目添加当前插件的NuGet依赖，版本为当前参数值。该参数可以用来解决Fody插件间接依赖MSBuild任务无效的问题，该参数为每个项目添加NuGet直接依赖。需要注意的是，为了加快Cli4Fody的执行速度，NuGet采用了`--no-restore`的方式进行安装。

    - `-m, --mode <overwrite|default>`

        操作模式，`--addin`的子配置项，默认`overwrite`，新建或重写以存在的`--addin`配置节点；`default`表示仅作为默认配置，如果`--addin`配置节点已存在则不进行任何修改。注意`-m, --mode`作为`--addin`的子配置项，必须在`--addin`之后指定。
        
        错误用法：`fody-cli MySolution.sln -m default --addin Rougamo`
        
        正确用法：`fody-cli MySolution.sln --addin Rougamo -m default`

    - `-n, --node <NODE>`

        为插件添加子节点，`--addin`的子配置项。对于多层级的节点，可以使用`:`表达多级节点，比如`Items:Item`表示当前插件配置节点下的`Items`节点下的`Item`节点，多次指定同一个节点路径表示追加。
        
        例：`fody-cli MySolution.sln --addin Pooling -n Items:Item -n Items:Item`
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

        为插件或节点添加属性，`--addin`和`--node`的子配置项。

        例：`fody-cli MySolution.sln --addin Pooling -a enabled=true -n Items:Item -a "pattern=method(* StringBuilder.Clear(..))" -n Items:Item -a stateless=Random`
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

        为插件或节点设置节点值，`--addin`和`--node`的子配置项。

        例：`fody-cli MySolution.sln --addin Pooling -n Inspects:Inspect -v "execution(* *(..))"`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Pooling enabled="true">
            <Inspects>
              <Inspect>execution(* *(..))</Inspect>
            </Inspects>
          </Pooling>
        </Weavers>
        ```

## 使用场景

Cli4Fody主要有两个应用场景：
1. 你平时在使用Fody插件的时候可能已经发现，自己的项目必须直接依赖Fody插件，间接依赖时Fody插件不生效，比如[项目A] -> [项目B] -> Rougamo.Fody，此时项目A中是可以使用Rougamo.Fody中定义的类型的，但在编译时并没有修改对应的MSIL，必须项目A直接依赖Rougamo.Fody才会生效。出现这种情况是因为Fody调用系列插件修改MSIL是通过在MSBuild管道中插入一个在编译后执行的任务实现的，Fody的NuGet包中的build目录下包含了添加MSBuild任务的配置。而在项目依赖中，默认是不会传递这些配置的，所以需要每个项目都直接依赖Fody插件。

    由于间接依赖时已经能够使用到Fody插件中的类型，所以我们很容易忘记直接依赖Fody插件，导致最终插件未生效，此时便可以通过Cli4Fody为解决方案中的每个项目添加直接依赖。下面的命令会为解决方案的每个项目添加Rougamo.Fody的直接依赖，并新增/修改FodyWeavers.xml文件。
    > fody-cli MySolution.sln --addin Rougamo -pv 4.0.4

2. 部分Fody插件是可以仅通过配置来完成其目标的，比如ConfigureAwait.Fody、Pooling.Fody等。对于这类插件，可以通过Cli4Fody直接完成配置。

    ```shell
    fody-cli MySolution.sln \
              --addin ConfigureAwait -pv 3.3.2 -a ContinueOnCapturedContext=false \
              --addin Pooling -pv 0.1.0 \
                  -n Inspects:Inspect -v "execution(* *..*Serivce.*(..))" \
                  -n Items:Item -a stateless=Random
    ```

### 推荐实践

Cli4Fody非常适配CI，可以在自动构建中统一配置，既可以轻松的完成对Fody插件直接依赖的查漏补缺，也可以轻松地配置无侵入式Fody插件。比如可以参考[DockerSample](https://github.com/inversionhourglass/Cli4Fody/tree/master/samples/DockerSample)项目，在Dockerfile中通过Cli4Fody确保Rougamo.Fody的直接依赖以及添加对ConfigureAwait的依赖并进行配置。

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

### Pooling零侵入实践

[Pooling](https://github.com/inversionhourglass/Pooling) 使用 Cli4Fody 实现[零侵入式对象池操作替换](https://github.com/inversionhourglass/Pooling?tab=readme-ov-file#%E9%9B%B6%E4%BE%B5%E5%85%A5%E5%BC%8F%E6%B1%A0%E5%8C%96%E6%93%8D%E4%BD%9C)。
