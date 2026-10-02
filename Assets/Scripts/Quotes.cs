using System.Collections.Generic;
using UnityEngine;

/// <summary>Surus sirasinda gokyuzunde beliren motivasyon cumleleri (4 dil).</summary>
public static class Quotes
{
    public static string Random(HashSet<int> used)
    {
        string[] list = Get(Loc.Language);
        if (used.Count >= list.Length) used.Clear();
        int i;
        do { i = UnityEngine.Random.Range(0, list.Length); } while (used.Contains(i));
        used.Add(i);
        return list[i];
    }

    static string[] Get(string lang)
    {
        switch (lang)
        {
            case "tr": return Tr;
            case "az": return Az;
            case "ru": return Ru;
            default: return En;
        }
    }

    static readonly string[] En =
    {
        "Keep going. You're closer than you think.",
        "Every hill is just a view you haven't earned yet.",
        "Fall seven times, stand up eight.",
        "Speed is nothing without courage.",
        "The road is tough. So are you.",
        "Don't stop when you're tired. Stop when you're done.",
        "Doubt kills more dreams than failure ever will.",
        "Small steps every day.",
        "Your only limit is you.",
        "Great things never came from comfort zones.",
        "Push harder than yesterday.",
        "The best view comes after the hardest climb.",
        "Believe you can and you're halfway there.",
        "Be stronger than your excuses.",
        "Difficult roads lead to beautiful places.",
        "Dream big. Ride hard.",
        "It always seems impossible until it's done.",
        "Don't wish for it. Work for it.",
        "You didn't come this far to only come this far.",
        "Fear is temporary. Regret is forever.",
        "Turn the noise into fuel.",
        "One more hill. One more try.",
        "Storms make trees take deeper roots.",
        "Progress, not perfection.",
        "The harder the battle, the sweeter the victory.",
        "Ride today like tomorrow is watching.",
        "Champions keep going when they can't.",
        "Make dust, not excuses.",
        "You are the driver of your life.",
        "Breathe. Focus. Go.",
    };

    static readonly string[] Tr =
    {
        "Devam et. Sandığından daha yakınsın.",
        "Her tepe, henüz hak etmediğin bir manzaradır.",
        "Yedi kez düş, sekiz kez kalk.",
        "Cesaret olmadan hız hiçbir şeydir.",
        "Yol zor. Sen de öylesin.",
        "Yorulunca değil, bitirince dur.",
        "Şüphe, başarısızlıktan çok hayal öldürür.",
        "Her gün küçük bir adım.",
        "Tek sınırın sensin.",
        "Büyük şeyler konfor alanından çıkmaz.",
        "Dünden daha çok zorla.",
        "En güzel manzara en zor tırmanışın ardındadır.",
        "İnanırsan yolun yarısı tamamdır.",
        "Bahanelerinden güçlü ol.",
        "Zor yollar güzel yerlere çıkar.",
        "Büyük hayal kur. Sert sür.",
        "Yapılana kadar her şey imkansız görünür.",
        "Dileme, çalış.",
        "Buraya kadar gelmek için gelmedin.",
        "Korku geçicidir. Pişmanlık kalıcı.",
        "Gürültüyü yakıta çevir.",
        "Bir tepe daha. Bir deneme daha.",
        "Fırtına ağacın köklerini derinleştirir.",
        "Mükemmellik değil, ilerleme.",
        "Savaş ne kadar zorsa zafer o kadar tatlı.",
        "Bugün, yarın izliyormuş gibi sür.",
        "Şampiyonlar yapamayınca da devam eder.",
        "Bahane değil, toz çıkar.",
        "Hayatının sürücüsü sensin.",
        "Nefes al. Odaklan. Git.",
    };

    static readonly string[] Az =
    {
        "Davam et. Düşündüyündən daha yaxınsan.",
        "Hər təpə hələ qazanmadığın bir mənzərədir.",
        "Yeddi dəfə yıxıl, səkkiz dəfə qalx.",
        "Cəsarətsiz sürət heç nədir.",
        "Yol çətindir. Sən də eləsən.",
        "Yorulanda yox, bitirəndə dayan.",
        "Şübhə arzuları uğursuzluqdan çox öldürür.",
        "Hər gün kiçik bir addım.",
        "Yeganə həddin özünsən.",
        "Böyük işlər rahatlıq zonasından çıxmır.",
        "Dünəndən daha çox çalış.",
        "Ən gözəl mənzərə ən çətin yoxuşdan sonradır.",
        "İnansan, yolun yarısı gedilib.",
        "Bəhanələrindən güclü ol.",
        "Çətin yollar gözəl yerlərə aparır.",
        "Böyük xəyal qur. Sərt sür.",
        "Edilənə qədər hər şey mümkünsüz görünür.",
        "Arzulama, çalış.",
        "Bura qədər gəlmək üçün gəlməmisən.",
        "Qorxu keçicidir. Peşmanlıq əbədi.",
        "Səs-küyü yanacağa çevir.",
        "Bir təpə də. Bir cəhd də.",
        "Fırtına ağacın köklərini dərinləşdirir.",
        "Mükəmməllik yox, irəliləyiş.",
        "Döyüş nə qədər çətinsə, qələbə o qədər şirindir.",
        "Bu gün sabah baxırmış kimi sür.",
        "Çempionlar bacarmayanda da davam edir.",
        "Bəhanə yox, toz çıxar.",
        "Həyatının sürücüsü sənsən.",
        "Nəfəs al. Fokuslan. Get.",
    };

    static readonly string[] Ru =
    {
        "Продолжай. Ты ближе, чем думаешь.",
        "Каждый холм — это вид, который ты ещё не заслужил.",
        "Упади семь раз, встань восемь.",
        "Скорость без смелости — ничто.",
        "Дорога трудна. Но и ты силён.",
        "Останавливайся не когда устал, а когда закончил.",
        "Сомнения губят больше мечтаний, чем неудачи.",
        "Маленький шаг каждый день.",
        "Твой единственный предел — ты сам.",
        "Великое не рождается в зоне комфорта.",
        "Жми сильнее, чем вчера.",
        "Лучший вид открывается после самого трудного подъёма.",
        "Поверь в себя — и половина пути пройдена.",
        "Будь сильнее своих отговорок.",
        "Трудные дороги ведут в красивые места.",
        "Мечтай смело. Гони жёстко.",
        "Всё кажется невозможным, пока не сделано.",
        "Не желай — работай.",
        "Ты прошёл столько не для того, чтобы остановиться.",
        "Страх временен. Сожаление вечно.",
        "Преврати шум в топливо.",
        "Ещё один холм. Ещё одна попытка.",
        "Бури делают корни деревьев глубже.",
        "Прогресс, а не совершенство.",
        "Чем труднее битва, тем слаще победа.",
        "Езжай сегодня так, будто завтра смотрит.",
        "Чемпионы продолжают, когда не могут.",
        "Поднимай пыль, а не отговорки.",
        "Ты водитель своей жизни.",
        "Вдох. Фокус. Вперёд.",
    };
}
