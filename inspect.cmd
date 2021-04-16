dotnet tool restore
dotnet jb inspectcode Caravela.sln --exclude=**\TestInputs\** --disable-settings-layers:GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal --swea=TRUE --output=obj\inspectcode.xml
