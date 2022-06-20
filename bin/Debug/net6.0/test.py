from importlib.resources import path
import json
import requests
import sys

args = str(sys.argv)

apiFile = open("C:/Users/matth/source/repos/Discord Bot 2/apiKey.txt", "r")
apiKey = apiFile.read()
apiFile.close()

global response

baseURL = 'https://www.bungie.net/Platform'
headers = {'X-API-Key': apiKey}

class PyClass:
    def SearchDestiny2(name, platform):
        name = args[0]
        platform = args [1]
        path = '/Destiny2/SearchDestinyPlayer/{platform}/{name}/'
        r = requests.get(baseURL+path, headers=headers)
        response = r.text

    