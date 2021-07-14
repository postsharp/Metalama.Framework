dotnet tool restore
dotnet jb cleanupcode -p=Custom Caravela.sln --toolset=16.0 --exclude=**\Caravela.Framework.Tests.PublicPipeline\Tests\**;**\Caravela.Framework.Tests.InternalPipeline\Tests\**  --disable-settings-layers:GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal 
