
from flask import Flask
from routes.greek_environment import greek_environment_interaction
from routes.character_interact import character_interact

app = Flask(__name__)

# Route for Greek environment interaction
@app.route('/interact/greek_environment', methods=['POST'])
def greek_environment_route():
    return greek_environment_interaction()

# Route for Character interaction
@app.route('/interact/character', methods=['POST'])
def character_route():
    return character_interact()

if __name__ == '__main__':
    app.run(port=5000)
