import msal
import jwt
import json
import requests
from datetime import date, datetime, timedelta

global tokenResponse
global requestHeaders
global tokenExpiry

tokenResponse = None
requestHeaders = None
tokenExpiry = None

# Step 1/2 - Insert your client id and certificate thumbprint here.  
clientID = 'INSERT_YOUR_CLIENT_ID'
thumbprint = 'INSERT_YOUR_THUMBPRINT'
# Step 2/2 - Add the certificate to the solution (with .pem extension) and make sure the certFile variable points to it 
certfile = '.\\INSERT_THE_NAME_OF_YOUR_CERTIFICATE_INCLUDING_EXTENSION.pem'

# Other constants
tenantID = '132d3d43-145b-4d30-aaf3-0a47aa7be073'
authority = 'https://login.microsoftonline.com/' + tenantID
scope = ['971c3c3b-0b7c-4991-bc10-6ac424c58779/.default']
apiUrl = 'https://hp-cloud-api-app-dev.azurewebsites.net/'


def getAccessToken(clientID, scope, authority, thumbprint, certfile):
    app = msal.ConfidentialClientApplication(clientID, 
        authority=authority, 
        client_credential={
            'thumbprint': thumbprint, 
            'private_key': open(certfile).read()
        }
    ) 
    result = app.acquire_token_for_client(scopes=scope)
    return result 

def getDecodedToken(accessToken):
    decodedAccessToken = jwt.decode(accessToken, options={'verify_signature': False})
    accessTokenFormatted = json.dumps(decodedAccessToken, indent=2)
    print('Token claims', accessTokenFormatted)
    return decodedAccessToken

def getMeasurementsForPowerLine():
    requestHeaders = {
        'Authorization': 'Bearer ' + tokenResponse['access_token'],
        'accept': 'text/plain'
    }

    neuronId = 717
    toDate = datetime.utcnow().astimezone()
    fromDate = datetime.utcnow().astimezone() - timedelta(days=7)

    url = '{}api/measurements?fromDateTime={}&toDateTime={}&neuronId={}'.format(apiUrl, getDateTimeStringForApi(fromDate), getDateTimeStringForApi(toDate), neuronId)

    try:
        response = requests.get(url, headers=requestHeaders)
        responseInJson = response.json()

        if not response.ok or response.ok and responseInJson['code'] != 200:
            print('Request failed', response.json())
            return

        responseCode = responseInJson['code']
        message = responseInJson['message']
        measurements = responseInJson['data']

        for measurement in measurements:
            print('Measurement at {}: Current: {}, Wire: {}, Housing: {}'.format(measurement['timeObserved'], measurement['current'], measurement['wireTemperature'], measurement['housingTemperature']))

        print('Code: {} - {} - found {} measurements'.format(responseCode, message, len(measurements)))
    except Exception as error:
        print('Exception occured', error)

def getDateTimeStringForApi(datetime):
    return datetime.strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'

try:
    try:
        # Get a new Access Token using Client Credentials Flow and a certificate
        tokenResponse = getAccessToken(clientID, scope, authority, thumbprint, certfile)

    except Exception as err:
        print('Error acquiring authorization token. Check your tenantID, clientID and certficate thumbprint.')
        print(err)

    # Read claims from the token
    decodedAccessToken = getDecodedToken(tokenResponse['access_token'])
    tokenExpiry = datetime.fromtimestamp(int(decodedAccessToken['exp'])) 
    timeToExpiry = tokenExpiry - datetime.now() 
    print('Token expires in ' + str(timeToExpiry.seconds / 60) + ' minutes at ' + str(tokenExpiry)) 

    isTokenValid = timeToExpiry.seconds > 0

    # Get data from API
    if isTokenValid:
        getMeasurementsForPowerLine()
        
    else:
        print('Need to get a new token')

except Exception as err:
    print(err)