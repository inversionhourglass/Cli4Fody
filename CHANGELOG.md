# Cli4Fody

中文 | [English](https://github.com/inversionhourglass/Cli4Fody/blob/master/README_en.md)

Cli4Fody是一个命令行工具，用于管理和修改`FodyWeavers.xml`文件。通过该工具，可以让部分Fody插件实现完全零侵入式的MSIL修改。

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

> fody MySolution.sln --addin ConfigureAwait -a ContinueOnCapturedContext=true

执行命令后的`FodyWeavers.xml`：

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <ConfigureAwait ContinueOnCapturedContext="true" />
</Weavers>
```

## 命令格式

> fody &lt;SolutionOrProjectPath&gt; [Options]

- `<solutionOrProjectPath>` 解决方案文件(*.sln)路径或项目文件(*.csproj)路径
- Options
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
        使用Cli4Fody执行`fody MyProject.csproj --order Rougamo,Pooling,_others_` 后得到如下`FodyWeavers.xml`：
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
        
        例：`fody MyProject.csproj --addin Rougamo`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Rougamo />
        </Weavers>
        ```

    - `-m, --mode <overwrite|default>`

        操作模式，`--addin`的子配置项，默认`overwrite`，新建或重写以存在的`--addin`配置节点；`default`表示仅作为默认配置，如果`--addin`配置节点已存在则不进行任何修改。注意`-m, --mode`作为`--addin`的子配置项，必须在`--addin`之后指定。
        
        正确用法：`fody MySolution.sln --addin Rougamo -m default`
        
        错误用法：`fody MySolution.sln -m default --addin Rougamo`

    - `-n, --node <NODE>`

        为插件添加子节点，`--addin`的子配置项。对于多层级的节点，可以使用`:`表达多级节点，比如`Items:Item`表示当前插件配置节点下的`Items`节点下的`Item`节点，多次指定同一个节点路径表示追加。
        
        例：`fody MySolution.sln --addin Pooling -n Items:Item -n Items:Item`
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

        例：`fody MySolution.sln --addin Pooling -a enabled=true -n Items:Item -a "pattern=method(* StringBuilder.Clear(..))" -n Items:Item -a stateless=Random`
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

        例：`fody MySolution.sln --addin Pooling -n Inspects:Inspect -v "execution(* *(..))"`
        ```xml
        <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
          <Pooling enabled="true">
            <Inspects>
              <Inspect>execution(* *(..))</Inspect>
            </Inspects>
          </Pooling>
        </Weavers>
        ```

## 零侵入代码织入案例

[Pooling](https://github.com/inversionhourglass/Pooling) 使用 Cli4Fody 实现[零侵入式对象池操作替换](https://github.com/inversionhourglass/Pooling?tab=readme-ov-file#%E9%9B%B6%E4%BE%B5%E5%85%A5%E5%BC%8F%E6%B1%A0%E5%8C%96%E6%93%8D%E4%BD%9C)。
