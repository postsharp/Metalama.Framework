dotnet tool restore
dotnet jb cleanupcode -p=Custom ..\Caravela.sln --exclude=**\Caravela.Framework.Tests.Integration\Tests\** --disable-settings-layers:GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal 
