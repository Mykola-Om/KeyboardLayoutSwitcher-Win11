using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyboardLayoutSwitcher
{
    public static class KeyMapper
    {
        private static readonly HashSet<char> englishVowels = new HashSet<char>
        {
            'a', 'e', 'i', 'o', 'u', 'y', 'A', 'E', 'I', 'O', 'U', 'Y'
        };

        private static readonly HashSet<string> commonEnglishWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "of", "and", "to", "a", "in", "for", "is", "on", "that", "by", "this", "with", "i", "you", "it", "not", "or", "be", "are", "from", "at", "as", "your", "all", "have", "new", "more", "an", "was", "we", "will", "home", "can", "us", "about", "if", "page", "my", "has", "search", "free", "but", "our", "one", "other", "do", "no", "information", "time", "they", "site", "he", "up", "may", "what", "which", "their", "news", "out", "use", "any", "there", "see", "only", "so", "his", "when", "contact", "here", "business", "who", "web", "also", "now", "help", "get", "pm", "view", "online", "first", "am", "been", "would", "how", "were", "me", "services", "some", "these", "click", "its", "like", "service", "than", "find", "price", "date", "back", "top", "people", "had", "list", "name", "just", "over", "state", "year", "day", "into", "email", "two", "health", "world", "next", "used", "go", "work", "last", "most", "products", "music", "buy", "data", "make", "them", "should", "product", "system", "post", "her", "city", "add", "policy", "number", "such", "please", "available", "copyright", "support", "message", "after", "best", "software", "then", "jan", "good", "video", "well", "where", "info", "rights", "public", "books", "high", "school", "through", "each", "links", "she", "review", "years", "order", "very", "privacy", "book", "items", "company", "read", "group", "need", "many", "user", "said", "does", "set", "under", "general", "research", "university", "january", "mail", "full", "map", "reviews", "program", "life", "know", "games", "way", "days", "management", "part", "could", "great", "united", "hotel", "real", "item", "international", "center", "ebay", "must", "store", "travel", "comments", "made", "development", "report", "off", "member", "details", "line", "terms", "before", "hotels", "did", "send", "right, type", "because", "local", "those", "using", "results", "office", "education", "national", "car", "design", "take", "posted", "internet", "address", "community", "within", "states", "area", "want", "phone", "dvd", "shipping", "reserved", "subject", "between", "forum", "family", "long", "based", "code", "show", "even", "black", "check", "special", "prices", "website", "index", "being", "women", "much", "sign", "file", "link", "open", "today", "technology", "south", "case", "project", "same", "pages", "version", "section", "own", "found", "sports", "house", "related", "security", "both", "county", "american", "photo", "game", "members", "power", "while", "care", "network", "down", "computer", "systems", "three", "total", "place", "end", "following", "download", "him", "without, per", "access", "think", "north", "resources", "current", "posts", "big", "media", "law", "control", "water", "history, pictures", "size", "art", "personal", "since", "including", "guide", "shop", "directory", "board", "location", "change", "white", "text", "small", "rating", "rate", "government", "children", "during", "return", "students", "shopping", "account", "times", "sites", "level", "digital", "profile", "previous", "form", "events", "love", "old", "john", "main", "call", "hours", "image", "department", "title", "description, insurance", "another", "why", "shall", "property", "class", "still", "money", "quality", "every", "listing", "content", "country", "private", "little", "visit", "save", "tools", "low", "reply", "customer", "december", "compare", "movies", "include", "college", "value", "article", "york", "man", "card", "jobs", "provide", "food", "source", "author", "different", "press", "learn", "sale", "around", "print", "course", "job, canada", "process, teen", "room", "stock", "training, too", "credit", "point, join", "science", "men", "categories", "advanced", "west", "sales", "look", "english", "team", "estate", "box", "conditions", "select", "windows", "photos", "thread", "week", "category", "note", "live", "large", "gallery", "table", "register", "however", "june", "october", "november", "market", "library", "really", "action", "start", "series", "model", "features", "air", "industry, plan", "human, provided", "required", "second", "hot", "accessories", "cost", "movie", "forums", "march", "september", "better", "say", "questions, july", "yahoo", "going", "medical", "test", "friend", "come", "server", "study", "application", "cart", "staff", "articles", "san", "feedback", "again", "play", "looking", "issues", "april", "never", "users", "complete", "street", "topic", "comment", "financial", "things", "working", "against", "standard", "tax", "person", "below", "mobile", "less", "got", "blog", "party", "payment", "equipment, login", "student, let", "programs", "offers", "legal", "above", "recent", "park", "stores, side", "act", "problem", "red", "give", "memory", "performance", "social", "august", "quote", "language", "story", "sell", "options", "experience", "rates", "create", "key", "body", "young", "america", "important", "field", "few", "east", "paper", "single", "age", "activities", "club", "example", "girls", "additional", "password", "latest", "something", "road", "gift", "question", "changes", "night", "hard", "texas, oct", "pay", "four", "poker", "status", "browse", "issue", "range", "building", "seller", "court", "february", "always", "result", "audio", "light", "write", "war", "offer", "blue", "groups", "easy", "given", "files, event", "release", "analysis", "request, fax, china", "making, picture", "needs", "possible", "might", "professional", "yet, month", "major", "star", "areas", "future", "space", "committee", "hand", "sun", "cards", "problems", "london", "washington", "meeting", "rss", "become", "interest", "id", "child", "keep", "enter", "california, share", "similar", "garden", "schools", "million", "added", "reference", "companies", "listed", "baby", "learning, energy", "run", "delivery, popular", "term", "film", "stories", "put", "computers", "journal", "reports", "try", "welcome", "central", "images", "president", "notice", "original", "head", "radio", "until", "cell", "color", "self", "council", "away", "includes", "track", "australia", "discussion", "archive", "once", "others", "entertainment", "agreement", "format", "least", "society", "months", "log", "safety", "friends", "sure", "faq", "trade", "edition", "cars", "messages", "marketing", "tell", "further", "updated", "association", "able", "having, provides", "david", "fun", "already", "green", "studies", "close", "common", "drive", "specific", "several", "gold", "living", "collection", "called", "short", "arts", "lot", "ask", "display", "limited", "powered", "solutions", "means", "director", "daily", "beach", "past", "natural", "whether", "due", "electronics", "five", "upon", "period", "planning, database", "says", "official", "weather", "land", "average", "done", "technical", "window", "france", "pro", "region", "island", "record", "direct", "microsoft", "conference", "environment", "records, district", "calendar", "costs", "style, url", "front", "statement", "update", "parts", "ever", "downloads", "early", "miles", "sound, resource", "present", "applications", "either", "ago", "document", "word", "works", "material", "bill", "written", "talk", "federal", "hosting", "rules", "final", "adult", "tickets", "thing", "centre", "requirements, via", "cheap", "kids", "finance", "true", "minutes", "else", "mark", "third", "rock", "gifts", "europe", "reading", "topics", "bad", "individual", "tips", "plus", "auto", "cover", "usually", "edit", "together, videos", "percent", "fast", "function", "fact", "unit", "getting, global", "tech", "meet", "far", "economic, player", "projects", "lyrics", "often", "subscribe, submit", "germany", "amount", "watch", "included", "feel", "though", "bank", "risk", "thanks", "everything", "deals", "various", "words", "linux", "production", "commercial", "james", "weight", "town", "heart", "advertising", "received", "choose", "treatment", "newsletter", "archives", "points", "knowledge", "magazine", "error", "camera", "girl", "currently", "construction", "toys", "registered", "clear", "golf", "receive", "domain", "methods", "chapter", "makes", "protection", "policies", "loan", "wide", "beauty", "manager", "india", "position", "taken", "sort", "listings", "models", "michael", "known", "half", "cases", "step", "engineering", "florida", "simple", "quick", "none", "wireless", "license", "paul", "friday", "lake", "whole", "annual", "published", "later, basic, sony", "shows", "corporate", "google", "church", "method", "purchase", "customers", "active", "response", "practice", "hardware", "figure", "materials", "fire", "holiday", "chat", "enough", "designed", "along", "among, death", "writing, speed", "html", "countries", "loss", "face", "brand", "discount", "higher", "effects", "created", "remember", "standards", "oil", "bit", "yellow", "political", "increase", "advertise, kingdom", "base", "near", "environmental", "thought", "stuff", "french", "storage", "japan", "doing, loans", "shoes", "entry"
        };

        private static readonly HashSet<char> ukrainianVowels = new HashSet<char>
        {
            '\u0430', '\u0435', '\u0438', '\u0456', '\u043E', '\u0443', '\u044F', '\u044E', '\u0454', '\u0457',
            '\u0410', '\u0415', '\u0418', '\u0406', '\u041E', '\u0423', '\u042F', '\u042E', '\u0404', '\u0407'
        };

        private static readonly HashSet<string> commonUkrainianWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ну", "це", "де", "ти", "ми", "ви", "як", "чи", "бо", "у", "в", "і", "на", "з", "та", "не", "що", "я", "до", "це", "для", "як", "він", "за", "є", "року", "ви", "його", "від", "ми", "був", "році", "про", "щоб", "або", "вони", "які", "також", "із", "а", "але", "ти", "коли", "було", "який", "під", "час", "чи", "вона", "мені", "може", "була", "після", "були", "якщо", "мене", "дуже", "тому", "її", "через", "так", "можна", "їх", "має", "бути", "яка", "багато", "ще", "по", "все", "цього", "де", "того", "років", "цей", "зі", "вас", "лише", "більше", "можуть", "те", "між", "будь", "ці", "вам", "ніж", "хто", "свою", "буде", "кілька", "всі", "один", "чому", "життя", "тут", "ця", "при", "часто", "без", "людей", "йому", "нас", "навіть", "мають", "деякі", "протягом", "сказав", "щодо", "ніколи", "інших", "їм", "сьогодні", "можете", "свого", "просто", "тобі", "повинні", "цьому", "свій", "допомогою", "завжди", "більш", "інші", "потрібно", "проти", "нам", "зараз", "оскільки", "зазвичай", "вже", "разом", "добре", "зробити", "себе", "свої", "потім", "своїх", "цю", "два", "можу", "хочу", "яку", "нього", "будувати", "використання", "немає", "них", "над", "перед", "цих", "люди", "три", "часу", "мій", "хоча", "щось", "думаю", "став", "яких", "знову", "тільки", "яке", "тебе", "той", "хотів", "одним", "такі", "рік", "день", "різних", "місце", "нічого", "всіх", "одного", "двох", "спочатку", "усі", "тепер", "початку", "тоді", "майже", "цієї", "знаю", "більшість", "трохи", "штат", "серед", "використовується", "роботи", "багатьох", "перший", "тим", "поки", "отримав", "використовувати", "раніше", "якого", "роботу", "там", "можливо", "особливо", "кількість", "дослідження", "системи", "участь", "дітей", "однак", "світу", "світі", "таких", "їй", "робити", "чином", "міста", "завдяки", "мав", "таким", "сім", "мати", "краще", "міг", "дві", "своїм", "зробив", "хоче", "вперше", "фільм", "роки", "ласка", "компанія", "різні", "повинен", "компанії", "повністю", "групи", "лікування", "іноді", "допомогти", "століття", "війни", "новий", "містить", "нью", "треба", "існує", "крім", "якому", "основі", "приблизно", "знайти", "моя", "роль", "слід", "швидко", "доларів", "близько", "кожен", "своєї", "одна", "саме", "ним", "біля", "здається", "своєму", "людини", "людина", "назвою", "цій", "якби", "своє", "школи", "деяких", "рішення", "тих", "роках", "собі", "історії", "створення", "народився", "місто", "шляхом", "отримати", "ваш", "можемо", "води", "подобається", "включаючи", "понад", "почав", "пізніше", "складається", "скільки", "проблеми", "відомий", "кількох", "розвитку", "досить", "будинок", "гроші", "справді", "найбільш", "неї", "працює", "дозволяє", "сказати", "трьох", "даних", "легко", "навколо", "шоу", "землі", "значення", "такий", "цим", "своїй", "знаходиться", "могли", "іншими", "здоров", "система", "частина", "навчання", "ті", "використовуються", "частиною", "чотири", "ось", "наші", "працювати", "нові", "усіх", "відповідно", "ніхто", "група", "вересня", "процес", "країни", "місті", "дня", "жовтня", "можливість", "жінок", "стала", "питання", "робить", "березня", "побачити", "спосіб", "розташований", "знав", "означає", "знає", "травня", "липня", "наприклад", "січня", "альбом", "першим", "червня", "бачив", "часом", "піти", "сполучених", "чого", "місця", "собою", "хтось", "програми", "великий", "своєю", "будинку", "незважаючи", "однією", "наш", "виробництва", "хочете", "називається", "квітня", "місці", "днів", "пішов", "населення", "зміни", "серпня", "наприкінці", "права", "додому", "частину", "мало", "своїми", "відбувається", "маю", "набагато", "правило", "важко", "якої", "хочеш", "фільму", "частини", "двома", "насправді", "збираєтеся", "чоловік", "батько", "смерті", "стати", "листопада", "ними", "всьому", "наступного", "думав", "грудня", "використовують", "мої", "одну", "кожного", "включає", "вплив", "перш", "мови", "воно", "завтра", "серії", "ваші", "занадто", "мали", "слова", "буду", "рівень", "знаєте", "нових", "теж", "уряд", "пропонує", "жінки", "вважається", "світ", "великі", "період", "видів", "команди", "необхідно", "мабуть", "яким", "нового", "давайте", "весь", "мною", "речі", "помер", "каже", "штату", "сезону", "першого", "районі", "забезпечення", "перші", "лютого", "управління", "говорити", "великих", "нею", "університету", "грошей", "таке", "самі", "ваша", "термін", "дані", "велика", "надто", "вид", "робота", "хвилин", "індії", "разу", "переважно", "рівні", "стає", "дії", "мільйонів", "наших", "стаття", "працював", "якій", "люблю", "використовує", "любить", "іншого", "випадку", "раз", "сам", "форми", "називають", "тварин", "почали", "діти", "будемо", "повернувся", "систему", "останні", "боку", "разів", "сили", "команда", "сталося", "основному", "тіла", "створити", "знати", "зовсім", "зображення", "варто", "надзвичайно", "живе", "застосування", "одне", "двері", "другий", "менше", "вчора", "безпеки", "менш", "історію", "увагу", "інформації", "лінії", "центрі", "список", "вдома", "результати", "інший", "вийшов", "їхні", "дні", "тобою", "мовою", "кінця", "одному", "включають", "нашої", "кількості", "волосся", "замість", "обличчя", "нижче", "мого", "рівня", "вашого", "полягає", "забезпечує", "постійно", "проект", "якості", "ринку", "інформацію", "країні", "метою", "моделі", "мою", "момент", "поблизу", "бізнес", "дерев", "школі", "написав", "чотирьох", "стали", "фільмі", "нову", "книгу", "історія", "така", "гри", "території", "незабаром", "більшості", "результаті", "нарешті", "типу", "перша", "відео", "відповідь", "право", "сезон", "частині", "точки", "парку", "партії", "наша", "походження", "велику", "значно", "різними", "повідомлення", "штатів", "машину", "збираюся", "чим", "минулого", "джон", "тож", "їсти", "ввечері", "стало", "колись", "зробили", "потрібна", "місяців", "зору", "туди", "організації", "тіло", "нашого", "компанією", "будівлі", "іншим", "класу", "віці", "безпосередньо", "визначення", "проблем", "сказали", "сильно", "загалом", "японії", "формі", "отже", "останній", "перших", "жінка", "сподіваюся", "книги", "широко", "вигляді", "мережі", "думку", "батька", "грати", "мала", "кожна", "продуктів", "вашу", "згідно", "честь", "оголосив", "ролі", "повітря", "зростання", "намагався", "нової", "містять", "прийшов", "сказала", "вулиці", "вами", "поговорити", "йти", "країн", "стосується", "штатах", "призначений", "щодня", "завдання", "закон", "захворювання", "обидва", "досвід", "види", "жити", "досліджень", "вирішив", "підтримки", "швидше", "січні", "принаймні", "існують", "модель", "уряду", "пацієнтів", "допомоги", "дає", "зрозуміти", "руки", "чоловіків", "важливо", "нова", "їжі", "великої", "головним", "світло", "зокрема", "галузі", "відповідає", "місць", "друга", "щойно", "шість", "очі", "купити", "друзів", "лікарні", "мова", "форму", "енергії", "перше", "церкви", "захисту", "правда", "членів", "шлях", "такого", "вранці", "всю", "процесу", "осіб", "починаючи", "достатньо", "проблема", "воду", "становить", "поверхні", "друг", "колір", "син", "отримання", "знайшов", "сюди", "далі", "жовтні", "отримала", "функції", "незавершена", "родини", "досі", "народження", "річки", "травні", "дати", "гра", "всередині", "виробництво", "куди", "ймовірно", "можеш", "списку", "світової", "країнах", "округу", "рух", "відома", "викликає", "залишається", "міст", "дерева", "першу", "дав", "готель", "вище", "доступ", "систем", "президент", "довелося", "відбулася", "великого", "вересні", "періоду", "поруч", "якими", "найкращий", "руху", "можливості", "березні", "шкіри", "мозку", "питань", "належить", "червні", "стан", "столітті", "такими", "чоловіка", "уже", "розвиток", "твій", "книга", "обох", "впевнений", "годин", "взяти", "знаєш", "назва", "відомі", "житті", "дитина", "само", "йде", "виглядає", "посаду", "надає", "допомагає", "всього", "тижня", "план", "слово", "центру", "другої", "роками", "працюють", "підходить", "університеті", "десять", "абсолютно", "армії", "купив", "отримали", "кімнаті", "повинна", "залежить", "росії", "нами", "парк", "рамках", "хотіли", "кажуть", "розташована", "школа", "клітин", "неможливо", "випадках", "основні", "згодом", "першому", "квітні", "програма", "довго", "ніч", "нещодавно", "звичайно", "брати", "здатність", "починається", "точно", "контракт", "запитання", "скажи", "події", "липні", "усе", "тиждень", "машини", "листопаді", "перевагу", "настільки", "продукти", "обладнання", "захід", "вважають", "послуги", "погано", "назад", "прямо", "мову", "мистецтва", "вдалося", "створив", "тип", "діяльності", "єдиний", "статті", "членом", "заснований", "оголосила", "президента", "роблять", "метод", "умови", "брат", "говорить", "заснована", "збільшення", "крові", "суду", "лютому", "години", "хочуть", "хіба", "одночасно", "існування", "доведеться", "схоже", "використовували", "проекту", "бачили", "почала", "грудні", "причини", "потрібні", "американський", "відкриття", "визначити", "походить", "зрештою", "ігор", "серпні", "послуг", "землю", "клітини", "підвищення", "ніби", "ефект", "стилі", "основних", "словами", "випущений", "впливу", "політики", "приєднався", "діяльність", "підтримку", "ночі", "потрібен", "провів", "цікаво", "часів", "виконання", "ради", "розширення", "культури", "давно", "відносно", "північній", "вважає", "спати", "влади", "дійсно", "змін", "всій", "складу", "створений", "дружина", "ліги", "розпочав", "вода", "кольору", "одяг", "методи", "використанням", "вибір", "машина", "думаєте", "кожному", "операції", "дівчина", "студентів", "відміну", "уникнути", "вимагає", "людьми", "першою", "форма", "моменту", "справи"
        };

        private static readonly HashSet<string> commonTlds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".com", ".net", ".org", ".edu", ".gov", ".ua", ".io", ".me", ".info", ".biz", ".dev", ".app", ".ai", ".uk", ".ru",
            ".env", ".json", ".yml", ".yaml", ".config", ".xml", ".cs", ".cpp", ".py", ".js", ".ts"
        };

        private static readonly Dictionary<char, char> engToUkrMap = BuildMap();

        private static readonly Dictionary<char, char> ukrToEngMap = BuildReverseMap();

        public static string ConvertWord(string word, bool isEnglishLayout)
        {
            StringBuilder correctedWord = new StringBuilder();

            foreach (char c in word)
            {
                if (isEnglishLayout && engToUkrMap.ContainsKey(c))
                    correctedWord.Append(engToUkrMap[c]);
                else if (!isEnglishLayout && ukrToEngMap.ContainsKey(c))
                    correctedWord.Append(ukrToEngMap[c]);
                else
                    correctedWord.Append(c);
            }

            return correctedWord.ToString();
        }

        public static bool IsWrongLayout(string word, bool isEnglishLayout, AppSettings settings, bool? lastWordIsEnglish = null)
        {
            if (IsCodeOrUrl(word))
            {
                return false;
            }

            string convertedWord = ConvertWord(word, isEnglishLayout);
            
            // Priority 1: Dictionary Comparison
            bool originalInDict = MatchesFrequentWord(word, isEnglishLayout);
            bool convertedInDict = MatchesFrequentWord(convertedWord, !isEnglishLayout);

            // If the converted word is a clear winner in the target language dictionary
            if (convertedInDict && !originalInDict) return true;
            
            // If the original word is a clear winner in its own language dictionary
            if (originalInDict && !convertedInDict) return false;

            // If BOTH are in the dictionary (ambiguity like ye/ну, ce/це), use context
            if (originalInDict && convertedInDict)
            {
                if (lastWordIsEnglish.HasValue)
                {
                    // If context strongly suggests the OTHER layout, switch
                    if (lastWordIsEnglish.Value != isEnglishLayout) return true;
                }
                return false; // Stay safe if no context or context matches current layout
            }

            if (settings != null && settings.IgnoredWords != null && settings.IgnoredWords.Contains(word))
            {
                return false;
            }

            int minimumWordLength = 2; 
            if (word.Length < minimumWordLength) return false;

            // Phonetic checks
            bool currentValid = IsPhoneticallyValid(word, isEnglishLayout);
            bool targetValid = IsPhoneticallyValid(convertedWord, !isEnglishLayout);

            if (!currentValid && targetValid) return true;
            if (currentValid && !targetValid) return false;

            // Priority 2: Vowel delta check
            int sourceVowelCount = CountVowels(word, isEnglishLayout ? englishVowels : ukrainianVowels);
            int targetVowelCount = CountVowels(convertedWord, isEnglishLayout ? ukrainianVowels : englishVowels);

            // Context-biased vowel/mapping thresholds
            int minimumVowelDelta = settings?.MinimumVowelDelta ?? 1;
            
            // If current word has NO vowels and target HAS vowels, it's a very strong indicator
            if (sourceVowelCount == 0 && targetVowelCount > 0 && word.Length >= 3) return true;

            // If context strongly suggests we are in the WRONG layout, we are more lenient with vowel delta
            if (lastWordIsEnglish.HasValue && lastWordIsEnglish.Value != isEnglishLayout)
            {
                minimumVowelDelta = 0; 
            }

            // Fallback to mapping percent and vowel delta
            int sourceMappedChars = CountMappedChars(word, isEnglishLayout ? engToUkrMap : ukrToEngMap);
            int minimumMappedPercent = Math.Max(1, Math.Min(100, settings?.MinimumMappedPercent ?? 80));
            int mappedThreshold = (int)Math.Ceiling(word.Length * minimumMappedPercent / 100.0);

            return sourceMappedChars >= mappedThreshold &&
                   targetVowelCount - sourceVowelCount >= minimumVowelDelta;
        }

        private static bool IsCodeOrUrl(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            // Specific case for 'env' (as command or filename prefix)
            if (word.Equals("env", StringComparison.OrdinalIgnoreCase)) return true;

            // Simple URL/Email patterns
            if (word.Contains("://") || word.StartsWith("www.") || word.Contains("@")) return true;

            // code indicators: underscores, backslashes, hashes
            if (word.Contains("_") || word.Contains("\\") || word.Contains("#")) return true;

            // Domain zone detection
            foreach (var tld in commonTlds)
            {
                if (word.EndsWith(tld, StringComparison.OrdinalIgnoreCase)) return true;
            }

            // ALL_CAPS or SHOUTING_CASE detection (common for constants/env vars)
            if (word.Length >= 3 && word.All(c => (char.IsUpper(c) || c == '_' || char.IsDigit(c)) && !char.IsLower(c)))
            {
                // Ensure it's not just a short Ukrainian word that happens to look like caps
                // (Most Ukrainian letters have multi-case, but let's be safe)
                return true;
            }

            // CamelCase detection: at least one capital letter NOT at the beginning
            for (int i = 1; i < word.Length; i++)
            {
                if (char.IsUpper(word[i]) && char.IsLower(word[i - 1])) return true;
            }

            // Dot detection (paths vs abbreviations vs layout letters)
            int lastDot = word.LastIndexOf('.');
            if (lastDot > 0 && lastDot < word.Length - 1)
            {
                // If the dot is followed by more than 1 char, it might be an extension
                if (word.Length - lastDot > 2) return true;
                
                // If it's something like .є or .ю, it's likely a typo'd dot in Ukrainian
                // but if it's .c or .h, it's likely a file/code
                char afterDot = word[lastDot + 1];
                if (afterDot == 'c' || afterDot == 'h' || afterDot == 'p' || afterDot == 'j') return true;
            }

            return false;
        }

        private static bool IsPhoneticallyValid(string word, bool isEnglish)
        {
            if (word.Length <= 1) return true;

            string lowerWord = word.ToLowerInvariant();
            int vowels = CountVowels(lowerWord, isEnglish ? englishVowels : ukrainianVowels);

            if (isEnglish)
            {
                // Words with no vowels are likely gibberish (ghfw, yfcn)
                if (vowels == 0 && word.Length >= 3) return false;

                // 'q' must be followed by 'u'
                if (lowerWord.Contains("q") && !lowerWord.Contains("qu")) return false;

                // Forbidden start clusters
                string[] forbiddenOnsets = { "tl", "dl", "vl", "mr", "ml", "sr", "gh", "fw", "yf", "yn" };
                foreach (var onset in forbiddenOnsets)
                    if (lowerWord.StartsWith(onset)) return false;

                // Forbidden combinations
                if (lowerWord.Contains("ghf") || lowerWord.Contains("ghw") || lowerWord.Contains("fwn")) return false;
            }
            else
            {
                // Words with no vowels in Ukrainian are impossible except for short ones like з, в
                if (vowels == 0 && word.Length >= 2) return false;

                // 'ь', 'й' cannot start a word
                if (lowerWord.StartsWith("\u044C")) return false; // ь
                if (lowerWord.StartsWith("\u0439") && lowerWord.Length > 1 && 
                    !"\u043E\u0443\u0442".Contains(lowerWord[1])) return false; // й (o, u, t are ok)

                // ь after vowel is illegal
                string ukrVowelsStr = "\u0430\u0435\u0438\u0456\u043E\u0443\u044F\u044E\u0454\u0457";
                for (int i = 1; i < lowerWord.Length; i++)
                    if (lowerWord[i] == '\u044C' && ukrVowelsStr.Contains(lowerWord[i - 1])) return false;

                // и at start is illegal
                if (lowerWord.StartsWith("\u0438")) return false;
            }

            return true;
        }

        public static bool IsLayoutWordCharacter(char character, bool isEnglishLayout)
        {
            if (char.IsLetter(character) || char.IsDigit(character))
            {
                return true;
            }

            // Technical symbols that should be part of a "word" context for heuristics
            if ("_./\\:@#-".Contains(character))
            {
                return true;
            }

            if (isEnglishLayout && engToUkrMap.TryGetValue(character, out char mappedUkrainianCharacter))
            {
                return char.IsLetterOrDigit(mappedUkrainianCharacter) || "_./\\:@#-".Contains(mappedUkrainianCharacter);
            }

            if (!isEnglishLayout && ukrToEngMap.TryGetValue(character, out char mappedEnglishCharacter))
            {
                return char.IsLetterOrDigit(mappedEnglishCharacter) || "_./\\:@#-".Contains(mappedEnglishCharacter);
            }

            return false;
        }

        private static bool MatchesFrequentWord(string word, bool isEnglishLayout)
        {
            string normalizedWord = NormalizeWord(word);
            if (string.IsNullOrEmpty(normalizedWord))
            {
                return false;
            }

            return isEnglishLayout
                ? commonEnglishWords.Contains(normalizedWord)
                : commonUkrainianWords.Contains(normalizedWord);
        }

        private static string NormalizeWord(string word)
        {
            return (word ?? string.Empty)
                .Trim()
                .Trim('\'', '"', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}')
                .ToLowerInvariant();
        }

        private static Dictionary<char, char> BuildMap()
        {
            Dictionary<char, char> map = new Dictionary<char, char>();
            AddMappings(map, "qwertyuiop[]", "\u0439\u0446\u0443\u043A\u0435\u043D\u0433\u0448\u0449\u0437\u0445\u0457");
            AddMappings(map, "asdfghjkl;'", "\u0444\u0456\u0432\u0430\u043F\u0440\u043E\u043B\u0434\u0436\u0454");
            AddMappings(map, "zxcvbnm,.", "\u044F\u0447\u0441\u043C\u0438\u0442\u044C\u0431\u044E");
            AddMappings(map, "QWERTYUIOP{}", "\u0419\u0426\u0423\u041A\u0415\u041D\u0413\u0428\u0429\u0417\u0425\u0407");
            AddMappings(map, "ASDFGHJKL:\"", "\u0424\u0406\u0412\u0410\u041F\u0420\u041E\u041B\u0414\u0416\u0404");
            AddMappings(map, "ZXCVBNM<>", "\u042F\u0427\u0421\u041C\u0418\u0422\u042C\u0411\u042E");
            return map;
        }

        private static Dictionary<char, char> BuildReverseMap()
        {
            Dictionary<char, char> reverseMap = new Dictionary<char, char>();
            foreach (KeyValuePair<char, char> pair in engToUkrMap)
            {
                reverseMap[pair.Value] = pair.Key;
            }

            return reverseMap;
        }

        private static void AddMappings(IDictionary<char, char> map, string source, string target)
        {
            for (int index = 0; index < source.Length; index++)
            {
                map[source[index]] = target[index];
            }
        }

        private static int CountVowels(string word, HashSet<char> vowels)
        {
            int count = 0;
            foreach (char c in word)
            {
                if (vowels.Contains(c))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMappedChars(string word, Dictionary<char, char> map)
        {
            int count = 0;
            foreach (char c in word)
            {
                if (map.ContainsKey(c))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
