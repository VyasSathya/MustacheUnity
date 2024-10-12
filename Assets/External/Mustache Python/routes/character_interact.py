from flask import request, jsonify
from utils.llm_api import call_llm

def character_interact():
    try:
        data = request.get_json()
        print(f"Received data: {data}")  # Log received data

        character_name = data.get('character_name')  # Extract character name
        interaction_type = data.get('interaction_type', 'dialogue')  # Extract interaction type
        player_message = data.get('player_message', '')  # Extract player's message

        # Log what was extracted
        print(f"Character: {character_name}, Interaction Type: {interaction_type}, Player Message: {player_message}")

        if not character_name:
            return jsonify({"error": "Character name must be provided."}), 400

        # Construct the prompt dynamically
        if interaction_type == 'dialogue':
            prompt = f"As {character_name}, respond to the following question: {player_message}. Provide an answer in the style of {character_name}, without narration or introductions."
        elif interaction_type == 'quest':
            prompt = f"{character_name} is assigning the player a quest. What is the quest?"
        else:
            prompt = f"Generate an interaction with {character_name} where the player engages in {interaction_type}."

        print(f"Generated prompt: {prompt}")  # Log the generated prompt

        response = call_llm(prompt)
        print(f"LLM Response: {response}")  # Log the LLM response

        return jsonify({'character_response': response})

    except Exception as e:
        print(f"Error occurred: {e}")  # Log the error
        return jsonify({"error": str(e)}), 500