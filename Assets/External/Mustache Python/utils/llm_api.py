import openai

def call_llm(prompt):
    # Use the chat-completions endpoint for chat models
    response = openai.ChatCompletion.create(
        model="gpt-4o-mini",  # Use the correct chat model
        messages=[
            {"role": "user", "content": prompt}
        ],
        max_tokens=150
    )

    return response.choices[0].message['content'].strip()