# Requirements
* .NET 5 SK: https://www.microsoft.com/net/download/windows

# Setup
1. Open [`Program.cs`](DotNetClient/Program.cs)
2. Set the `clientId` property to the application/client ID value you received earlier
3. Add the .pfx certificate to the root of the solution
4. Right click certificate --> properties --> set "Copy to output folder: always"
5. Set the `PfxCertificatePath` property to the relative path to the .pfx certificate (include file extension)
6. Set the `CertificatePassword` property
7. Set the `useDeveloperApi` property to false if you want production data

# Run
Option 1: Open the solution in Visual Studio and run the application.

Option 2: Run the program in a terminal with 
'''
cd DotNetClient\
dotnet run
'''

