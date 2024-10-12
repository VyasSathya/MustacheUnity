
from flask import request, jsonify
from utils.llm_api import call_llm

def greek_environment_interaction():
    data = request.get_json()
    object_type = data.get('object_type')  # Could be "building", "character", or "object"
    object_name = data.get('object_name')  # Specific object like "Temple", "Socrates", etc.

    prompt = f"The player is interacting with a {object_type} called {object_name} in ancient Greece. Describe the environment, the object's characteristics, and any historical details."

    response = call_llm(prompt)

    return jsonify({"response": response})
