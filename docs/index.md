# Powershell Configuration Substitution

## Setup

### PowerShell Gallery
```powershell
> Install-Module configuration-substitution
> Import-Module configuration-substitution
```

### GitHub

```terminal
> git clone https://github.com/tosoikea/PowerShell-Configuration-Substitution.git
> Import-Module .\configuration-substitution
```
## Test functionality
```powershell
> $container = @{myVar="TEst"}
> ConvertTo-NamingArray -SchemeString "{myVar(?lower)(?0)}" -InputObject $container

TEst
t
```


