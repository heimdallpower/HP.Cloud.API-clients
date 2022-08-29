# Requirements
* [Python 3](https://www.python.org/downloads/) 
* Install the required packages listed below with pip `pip3 install -r requirements.txt`

```

Package                 Version
----------------------- ----------
msal                    1.12.0
PyJWT                   2.1.0
requests                2.23.0
```

# Setup
1. Open [`main.py`](main.py)
2. Set the `clientId` property to the application/client ID value you received earlier
3. Set the `thumbprint` property to the thumbprint of the certificate
4. Set the `pathToCertificatePrivateKey` property to the path to the .pem private key you generated or received earlier (include file extension)
4. Set the `useDeveloperApi` property to false if you want production data

# Run

Run the program with `python main.py`

Debugging information will be displayed in PythonClient/PythonClientLogger.log file