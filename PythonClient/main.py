from heimdallAPI import HeimdallAPI

def main() -> None:
    client_id = "INSERT CLIENT_ID HERE"
    thumbprint = 'INSERT THUMBPRINT HERE'
    pathToCertificatePrivateKey = "INSERT PATH TO CERTIFICATE PRIVATE KEY HERE\\FILENAME.pem"
    heimdall_api = HeimdallAPI(client_id=client_id, thumbprint=thumbprint, path_to_certificate=pathToCertificatePrivateKey)

if __name__ == "__main__":
    main()