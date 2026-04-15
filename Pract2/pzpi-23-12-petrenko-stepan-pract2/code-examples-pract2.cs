Запити ШІ:

Можеш написати приклад теоритичної інтеграції з ші сервісом xAI Grok API що буде перевіряти повідомлення користувача на корректність і доцільність

Напиши приклад програмного коду що демострує простоту бізнес логіки

Напиши приклад MTProto RPC
Програмний код
1) Код перевірки на доцільність повіомлення користувача

import os
import json
from dotenv import load_dotenv
from openai import AsyncOpenAI          # Асинхронний клієнт
from aiogram import Bot, Dispatcher, types
from aiogram.filters import Command
from aiogram import F

load_dotenv()

# === Налаштування ===
TELEGRAM_TOKEN = os.getenv("TELEGRAM_TOKEN")
XAI_API_KEY = os.getenv("XAI_API_KEY")

bot = Bot(token=TELEGRAM_TOKEN)
dp = Dispatcher()

# xAI Grok клієнт
grok_client = AsyncOpenAI(
    api_key=XAI_API_KEY,
    base_url="https://api.x.ai/v1"
)

# Системний промпт для Grok (дуже важливий!)
SYSTEM_PROMPT = """
Ти — строгий, але доброзичливий модератор і редактор. 
Перевір повідомлення користувача за двома критеріями:

1. КОРЕКТНІСТЬ:
   - Орфографія та граматика українською/російською/англійською.
   - Фактична точність (якщо є факти).
   - Логічна послідовність і відсутність суперечностей.

2. ДОЦІЛЬНІСТЬ:
   - Чи релевантне повідомлення контексту чату?
   - Чи корисне/конструктивне?
   - Чи не містить спаму, токсичності, реклами, образ?
   - Чи доцільно надсилати саме зараз?

Поверни ТІЛЬКИ валідний JSON (без додаткового тексту!):
{
  "correctness_score": 0-10,
  "expediency_score": 0-10,
  "verdict": "OK" | "CORRECT" | "IMPROVE" | "BLOCK",
  "issues": ["короткий список проблем"],
  "suggestion": "пропозиція виправленого повідомлення або пояснення",
  "explanation": "коротке пояснення для користувача"
}
"""

@dp.message(F.text)  # Реагуємо на будь-яке текстове повідомлення
async def check_message(message: types.Message):
    user_text = message.text.strip()
    if not user_text or len(user_text) < 3:
        return

    try:
        # Запит до Grok API
        response = await grok_client.chat.completions.create(
            model="grok-4.20-reasoning",          # або grok-4.20-non-reasoning для швидкості
            messages=[
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user", "content": f"Перевір це повідомлення:\n\n{user_text}"}
            ],
            max_tokens=800,
            temperature=0.3,          # низька температура для стабільності
            response_format={"type": "json_object"}   # примушуємо JSON
        )

        grok_result = json.loads(response.choices[0].message.content)

        # Логіка реакції бота
        verdict = grok_result.get("verdict", "IMPROVE")
        suggestion = grok_result.get("suggestion", "")
        explanation = grok_result.get("explanation", "")

        if verdict == "OK":
            await message.reply("✅ Повідомлення коректне та доцільне!")
        elif verdict == "CORRECT":
            await message.reply(f"🔧 Виправлено:\n{suggestion}\n\n{explanation}")
        elif verdict == "IMPROVE":
            await message.reply(f"⚠️ Рекомендації:\n{explanation}\n\nПропозиція: {suggestion}")
        elif verdict == "BLOCK":
            await message.delete()
            await message.answer(f"🚫 Повідомлення заблоковано.\nПричина: {explanation}")

    except Exception as e:
        await message.reply(f"❌ Помилка перевірки: {str(e)[:100]}")

# Запуск бота
async def main():
    await dp.start_polling(bot)

if __name__ == "__main__":
    import asyncio
    asyncio.run(main())

2) Програмний код що демострує простоту бізнес логіки
import requests
TOKEN = "YOUR_BOT_TOKEN"
def send_message(chat_id, text):
    url = f"https://api.telegram.org/bot{TOKEN}/sendMessage"
    payload = {"chat_id": chat_id, "text": text, "parse_mode": "HTML"}
    response = requests.post(url, json=payload)  # MTProto Gateway обробляє внутрішньо
    return response.json()

3) Приклад MTProto RPC
messages.sendMessage#d1d34c26 flags:INT peer:InputPeer message:string

