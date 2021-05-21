git subtree add --prefix .engineering https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash
New-Item -Path ..\.editorconfig -ItemType SymbolicLink -Value .\.editorconfig
New-Item -Path ..\Directory.Build.props -ItemType SymbolicLink -Value .\Directory.Build.props
New-Item -Path ..\Directory.Build.targets -ItemType SymbolicLink -Value .\Directory.Build.targets