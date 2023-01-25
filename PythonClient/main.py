from Enums.enums import DLRType
from heimdallAPI import HeimdallAPI
from heimdallAPI import logging

def main() -> None:
    client_id = "INSERT CLIENT ID HERE"
    client_secret = 'INSERT CLIENT SECRET HERE'
    use_dev_api = True #Set to false to use production data
    heimdall_api = HeimdallAPI(client_id=client_id, client_secret=client_secret, use_dev_api=use_dev_api)
    heimdall_api.get_aggregated_current_for_line(0)
    heimdall_api.get_dlr_for_line(0, DLRType.HP)
    logging.info("*"*65)

if __name__ == "__main__":
    main()