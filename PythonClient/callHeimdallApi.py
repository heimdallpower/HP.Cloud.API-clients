from enum import Enum
import msal
import jwt
import json
import requests
import urllib.parse
from datetime import date, datetime, timedelta
global tokenResponse
global requestHeaders
global tokenExpiry

tokenResponse = None
requestHeaders = None
tokenExpiry = None

# Insert proper variables here
clientID = 'INSERT_VARIABLE_HERE'
thumbprint = 'INSERT_VARIABLE_HERE'
pathToCertificatePrivateKey = '.\\INSERT_PATH_HERE.pem'

# Other constants
tenantID = '132d3d43-145b-4d30-aaf3-0a47aa7be073'
authority = 'https://login.microsoftonline.com/' + tenantID
scope = ['8ecd41dd-9d79-4440-b029-8ea602733d60/.default']
apiUrl = 'https://api.heimdallcloud.com/'

class MeasurementType(str, Enum):
    Current = 'Current'
    WireTemperature = 'WireTemperature'
class IntervalDuration(str, Enum):
    Every5Minutes = 'PT5M'
    EveryDay = 'P1D'
class AggregationType(str, Enum):
    MIN = 'Min'
    MAX = 'Max'
    AVERAGE = 'Average'

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

def getLineNames():
    requestHeaders = {
        'Authorization': 'Bearer ' + tokenResponse['access_token'],
        'accept': 'text/plain'
    }

    url = apiUrl + 'api/beta/lines'

    try:
        print('Sending request: ', url,'\n')
        response = requests.get(url, headers=requestHeaders)
        responseInJson = response.json()

        if not response.ok:
            print('Request failed', response.json())
            return

        lines = responseInJson['data']

        for line in lines:
            print('You can request data for the following lines and line spans:\nName: {}\nOwner: {}\nLine spans in line: {}\n'.format(line['name'], line['owner'], line['lineSpans']))

        print('Message: {}. Found {} lines'.format(responseInJson['message'], len(lines)))
        return lines
    except Exception as error:
        print('Exception occured', error)

def getAggregatedCurrentForLine(lineName):
    requestHeaders = {
        'Authorization': 'Bearer ' + tokenResponse['access_token'],
        'accept': 'text/plain'
    }

    neuronId = 703
    toDate = datetime.utcnow().astimezone()
    fromDate = datetime.utcnow().astimezone() - timedelta(days=7)

    url = '{}api/beta/aggregated-measurements?fromDateTime={}&toDateTime={}&lineName={}&aggregationType={}&intervalDuration={}&measurementType={}'.format(
        apiUrl, 
        getDateTimeStringForApi(fromDate), 
        getDateTimeStringForApi(toDate), 
        urllib.parse.quote_plus(lineName, encoding='UTF-8'),
        AggregationType.MAX,
        IntervalDuration.EveryDay,
        MeasurementType.Current
    )

    try:
        print('Sending request: ', url, '\n')
        response = requests.get(url, headers=requestHeaders)
        responseInJson = response.json()

        if not response.ok:
            print('Request failed', response.json())
            return

        print('Response message:', responseInJson['message'])
        aggregatedMeasurements = responseInJson['data']

        for measurement in aggregatedMeasurements:
            print('Current at {}: {}A'.format(
                measurement['intervalStartTime'], 
                measurement['value']
            ))

        print('Got {} measurements in response'.format(len(aggregatedMeasurements)))
    except Exception as error:
        print('Exception occured', error)

def getDateTimeStringForApi(datetime):
    return datetime.strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'

try:
    try:
        print('Hello Heimdall!')
        # Get a new Access Token using Client Credentials Flow and a certificate
        tokenResponse = getAccessToken(clientID, scope, authority, thumbprint, pathToCertificatePrivateKey)

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
        lineNames = getLineNames()
        if len(lineNames) < 0:
            print("Didn't find any lines")
        else:
            chosenLineName = lineNames[0]['name']
            print('Requesting aggregated current data for the line:', chosenLineName, '\n')
            getAggregatedCurrentForLine(chosenLineName)
        
    else:
        print('Need to get a new token')

except Exception as err:
    print(err)

