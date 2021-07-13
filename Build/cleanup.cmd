dotnet tool restore
dotnet jb cleanupcode -p=Custom Caravela.sln --exclude=**\Caravela.Framework.Tests.PublicPipeline\Tests\**;**\Caravela.Framework.Tests.InternalPipeline\Tests\**  --disable-settings-layers:GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal 
