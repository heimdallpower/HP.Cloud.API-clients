# Requirements
Maven 3.8.1
Java

# Setup

1. Open the [`src\main\resources\application.properties`](src\main\resources\application.properties) class
2. Set the `CLIENT_ID` property to the application/client ID value you received earlier
3. Set the `PKCS8_PRIVATE_KEY_PATH` property to the path to the PKCS8 private key you generated or received earlier
4. Set the `CRT_CERTIFICATE_PATH` property to the path to the .crt certificate you generated or received earlier

# Run

You can test the sample directly by running the main method of CallHeimdallAPI.java from your IDE.

From your shell or command line:

- `$ mvn clean compile assembly:single`

This will generate a `msal-client-credential-certificate-1.0.0.jar` file in your /targets directory. Run this using your Java executable like below:

- `$ java -jar msal-client-credential-certificate-1.0.0.jar`


After running, the application should retrieve an access token and use it to call the Heimdall API. 