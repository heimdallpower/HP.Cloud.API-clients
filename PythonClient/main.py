from Enums.enums import DLRType
from heimdallAPI import HeimdallAPI
from heimdallAPI import logging
def main() -> None:
    client_id = "3d7bc9e8-0602-4285-b53d-c5e3db18785b" #"INSERT CLIENT ID HERE"
    thumbprint = "95058c12a732d2b48d754f3b81115cb6c7dc3e6a" #'INSERT THUMBPRINT HERE'
    path_to_certificate_private_key = "C:\\Users\\LucaMancini\\Desktop\\certsForPythonAPI\\certificatePrivateKey.pem"# "INSERT//PATH//TO//CERTIFICATE//HERE//FILENAME.PEM"
    use_dev_api = True #Set to false to use production data
    heimdall_api = HeimdallAPI(client_id=client_id, thumbprint=thumbprint, path_to_certificate=path_to_certificate_private_key, use_dev_api=use_dev_api)
    heimdall_api.get_aggregated_current_for_line(45)
    heimdall_api.get_dlr_for_line(45, DLRType.HP)
    logging.info("*"*65)

if __name__ == "__main__":
    main()