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

logging.basicConfig(filename="PythonClient/PythonClientLogger.log", encoding= "utf-8", format='%(asctime)s %(levelname)-8s %(message)s', level=logging.INFO)

class HeimdallAPI:
    TENANT_ID = "132d3d43-145b-4d30-aaf3-0a47aa7be073"
    
    def __init__(self, client_id: str, thumbprint: str, path_to_certificate: str, use_dev_api : bool = True) -> None:
        self.client_id = client_id
        self.thumbprint = thumbprint
        self.path_to_certificate = path_to_certificate
        self.api_url = "https://api.heimdallcloud-dev.com" if use_dev_api else "https://api.heimdallcloud.com"
        self.scope = ['6b9ba5c0-4a21-4263-bbf5-8c4e30c0ee1b/.default'] if use_dev_api else ['aac6dec0-4c1b-4565-a825-5bb9401a1547/.default']
        self.authority = f"https://login.microsoftonline.com/{self.TENANT_ID}"
        
        self.access_token = self.get_access_token()["access_token"]
        self.requestHeaders = {
                                'Authorization': 'Bearer ' + self.access_token,
                                'accept': 'text/plain'
                              }
        self.decoded_token = self.get_decoded_token()
        self.lines = self.get_lines()

    def get_access_token(self) -> dict:
        with open(self.path_to_certificate, "r") as certificate_file:
            private_key = certificate_file.read()
            certificate_file.close()

        client_credential = {
            "thumbprint" : self.thumbprint,
            "private_key": private_key
        }
        
        try:
            client_app = msal.ConfidentialClientApplication(self.client_id, authority=self.authority, client_credential=client_credential)
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
            response = requests.get(url, headers=self.requestHeaders)
            response_json = response.json()

            if not response.ok:
                logging.error(f"Lines Request failed{response_json}")
                raise Exception("Network Request Failed")

            lines = response_json["data"]
            message = f"Message: {response_json['message']}\n\nFound {len(lines)} lines\n"
            
            logging.info(message)
            print(message) 
            print(f"{json.dumps(lines, indent=4)} \n\nrequest data with the ids of lines, spans, and span phases from the response above")

        except Exception as error:
            logging.error(f"{error}")
            logging.error("*"*65)

        return lines
    
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
                    AggregationType.MAX,
                    IntervalDuration.EveryDay,
                    MeasurementType.Current
                )
        try:
            logging.info(f"Sending request to {url}")
            response = requests.get(url, headers=self.requestHeaders)
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
                    IntervalDuration.EveryDay,
                )
        try:
            logging.info(f"Sending request to {url}")
            response = requests.get(url, headers=self.requestHeaders)
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