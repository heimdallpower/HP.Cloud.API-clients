import msal
import jwt
import json
import logging
import requests
import urllib.parse
from enum import Enum
from datetime import datetime, timedelta
from Enums.enums import MeasurementType, AggregationType, IntervalDuration, DLRType
from CustomException.incorrectAccessTokenError import IncorrectAccessTokenError

logging.basicConfig(filename="PythonClientLogger.log", encoding= "utf-8", format='%(asctime)s %(levelname)-8s %(message)s', level=logging.INFO)

#SSL verify is set to False in requests, ignoring verbose messages
requests.packages.urllib3.disable_warnings()

class HeimdallAPI:    
    def __init__(self, client_id: str, client_secret: str, use_dev_api : bool = True) -> None:
        self.client_id = client_id
        self.client_secret = client_secret
        self.policy = "B2C_1A_CLIENTCREDENTIALSFLOW"
        self.instance = "https://hpadb2cdev.b2clogin.com" if use_dev_api else "https://hpadb2cprod.b2clogin.com"
        self.domain = "hpadb2cdev.onmicrosoft.com" if use_dev_api else "hpadb2cprod.onmicrosoft.com"
        self.api_url = "https://api.heimdallcloud-dev.com" if use_dev_api else "https://api.heimdallcloud.com"
        self.backend_client_id = "f2fd8894-ae2e-4965-8318-e6c6781b5b80" if use_dev_api else "dc5758ae-4eea-416e-9e61-812914d9a49a"
        self.scope = [f'https://{self.domain}/{self.backend_client_id}/.default']
        self.authority = f'{self.instance}/tfp/{self.domain}/{self.policy}'
        
        self.access_token = self.get_access_token()["access_token"]
        self.requestHeaders = {
                                'Authorization': 'Bearer ' + self.access_token,
                                'accept': 'text/plain'
                              }
        self.decoded_token = self.get_decoded_token()
        self.lines = self.get_lines()

    def get_access_token(self) -> dict:
        
        try:
            client_app = msal.ConfidentialClientApplication(self.client_id, authority=self.authority, client_credential=self.client_secret)
            client_token = client_app.acquire_token_for_client(scopes=self.scope)
            if ("error" or "error_description") in client_token:
                raise IncorrectAccessTokenError
            
            logging.info(f"access_token: {client_token}")

        except (Exception, IncorrectAccessTokenError) as error:
            logging.error(f"{error}")
            logging.error("*"*65)
            raise error

        return client_token
    
    def get_decoded_token(self) -> str:
        try:
            decoded_token = jwt.decode(self.access_token, options={"verify_signature": False})
            formatted_access_token = json.dumps(decoded_token, indent=4)
            token_expiry = datetime.fromtimestamp(int(decoded_token["exp"]))
            time_until_expiry = token_expiry - datetime.now()
            logging.info(f"Token Claims \n{formatted_access_token}")
            logging.info(f"Token expires in {time_until_expiry / 60} minutes, at {token_expiry}")

        except Exception as error:
            logging.error(f"{error}")
            logging.error("*"*65)

        return decoded_token
    
    def get_lines(self) -> list:
        url = f"{self.api_url}/api/v1/lines"
        try:
            logging.info(f"Sending request to {url}")
            response = requests.get(url, headers=self.requestHeaders,verify=False)
            response_json = response.json()

            if not response.ok:
                logging.error(f"Lines Request failed{response_json}")
                raise Exception("Network Request Failed")

            lines = response_json["data"]
            message = f"Message: {response_json['message']}\n\nFound {len(lines)} lines\n"
            
            logging.info(message)
            print(message) 
            print(f"{json.dumps(lines, indent=4)} \n\nrequest data with the ids of lines, spans, and span phases from the response above")

            return lines

        except Exception as error:
            logging.error(f"{error}")
            logging.error("*"*65)
    
    def get_aggregated_current_for_line(self, line_index: int) -> None:
        if not self.lines:
            logging.error("No lines were returned")
            logging.error("*"*65)
            raise Exception("No Lines Returned")

        chosen_line = self.lines[line_index]
        logging.info(f"Requesting aggregated current data for the line: {chosen_line['name']}")
        from_date = (datetime.utcnow().astimezone() - timedelta(days=7)).strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'
        to_date = datetime.utcnow().astimezone().strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'

        url = "{}/api/v1/aggregated-measurements?fromDateTime={}&toDateTime={}&lineId={}&aggregationType={}&intervalDuration={}&measurementType={}".format(
                    self.api_url,
                    from_date,
                    to_date,
                    chosen_line["id"],
                    AggregationType.MAX.value,
                    IntervalDuration.EveryDay.value,
                    MeasurementType.Current.value
                )
        try:
            logging.info(f"Sending request to {url}")
            response = requests.get(url, headers=self.requestHeaders,verify=False)
            response_json = response.json()

            if not response.ok:
                logging.error(f"Measurement Request failed{response_json}")
                raise Exception("Network Request Failed")

            aggregated_measurements = response_json["data"]
                     
            message = f"Measurement Response message: {response_json['message']}\n\nReceived {len(aggregated_measurements)} measurements in response\n" 
            logging.info(message)
            print(message)

            for measurement in aggregated_measurements:
                interval_start_time = measurement["intervalStartTime"]
                current_value = measurement["value"]
                
                print(f"Current at {interval_start_time}: {current_value} A")
                
        except Exception as error:
            logging.error(f"{error}")
            logging.error("*"*65)

    def get_dlr_for_line(self, line_index: int, dlr_type: Enum) -> None:
        if not self.lines:
            logging.error("No lines were returned")
            logging.error("*"*65)
            raise Exception("No Lines Returned")

        chosen_line = self.lines[line_index]
        logging.info(f"Requesting DLR data for the line: {chosen_line['name']}")
        from_date = (datetime.utcnow().astimezone() - timedelta(days=7)).strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'
        to_date = datetime.utcnow().astimezone().strftime('%Y-%m-%dT%H:%M:%S.%f')+'Z'

        url = '{}/api/beta/dlr/aggregated-dlr?fromDateTime={}&toDateTime={}&lineName={}&dlrType={}&intervalDuration={}'.format(
                    self.api_url,
                    from_date,
                    to_date, 
                    urllib.parse.quote_plus(chosen_line['name'], encoding='UTF-8'),
                    dlr_type,
                    IntervalDuration.EveryDay.value,
                )
        try:
            logging.info(f"Sending request to {url}")
            response = requests.get(url, headers=self.requestHeaders,verify=False)
            response_json = response.json()

            if not response.ok:
                logging.error(f"DLR Request failed{response_json}")
                raise Exception("Network Request Failed")

            aggregated_measurements = response_json["data"]
                     
            message = f"DLR Response message: {response_json['message']}\n\nReceived {len(aggregated_measurements)} measurements in response\n" 
            logging.info(message)
            print(message)

            for measurement in aggregated_measurements:
                interval_start_time = measurement["intervalStartTime"]
                current_value = measurement["ampacity"]
                
                print(f"Dynamic Line Rating at {interval_start_time}: {current_value} A")

        except Exception as error:
            logging.error(f"{error}")
            logging.error("*"*65)
