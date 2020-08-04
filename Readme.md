# <img src="/Signet.Fody.png" height="30px"> Signet.Fody

[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg)](https://gitter.im/Fody/Fody)
[![NuGet Status](https://img.shields.io/nuget/v/Signet.Fody.svg)](https://www.nuget.org/packages/Signet.Fody)

Adds a AssemblyInformationalVersionAttribute to an assembly.


### This is an add-in for [Fody](https://github.com/Fody/Home)

**It is expected that all developers using Fody either [become a Patron on OpenCollective](https://opencollective.com/fody/contribute/patron-3059), or have a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-fody?utm_source=nuget-fody&utm_medium=referral&utm_campaign=enterprise). [See Licensing/Patron FAQ](https://github.com/Fody/Home/blob/master/pages/licensing-patron-faq.md) for more information.**


## Usage

See also [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md).


### NuGet Installation

Install the [Signet.Fody NuGet package](https://nuget.org/packages/Signet.Fody) and update the [Fody NuGet package](https://nuget.org/packages/Fody):

```
PM> Install-Package Fody
PM> Install-Package Signet.Fody
```

The `Install-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<Signet />` to [FodyWeavers.xml](https://github.com/Fody/Home/blob/master/pages/usage.md#add-fodyweaversxml)

```xml
<Weavers>
  <Signet />
</Weavers>
```


## Icon

Icon courtesy of [Icon Font](https://www.iconfont.cn/search/index?searchType=icon&q=Signet)
