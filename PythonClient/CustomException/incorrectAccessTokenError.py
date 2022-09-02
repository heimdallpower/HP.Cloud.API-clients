class IncorrectAccessTokenError(Exception):
    
    def __init__(self, message="Incorrect Access Token"):
        self.message = message
        super().__init__(self.message)