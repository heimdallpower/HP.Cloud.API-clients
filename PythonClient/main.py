from Enums.enums import DLRType
from heimdallAPI import HeimdallAPI
from heimdallAPI import logging

def main() -> None:
    client_id = "INSERT CLIENT ID HERE"
    thumbprint = 'INSERT THUMBPRINT HERE'
    path_to_certificate_private_key = "INSERT//PATH//TO//CERTIFICATE//HERE//FILENAME.PEM"
    use_dev_api = True #Set to false to use production data
    heimdall_api = HeimdallAPI(client_id=client_id, thumbprint=thumbprint, path_to_certificate=path_to_certificate_private_key, use_dev_api=use_dev_api)
    heimdall_api.get_aggregated_current_for_line(0)
    heimdall_api.get_dlr_for_line(0, DLRType.HP)
    logging.info("*"*65)

if __name__ == "__main__":
    main()