using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    // Data/quran-uthmani-word-parts.txt
    //LOCATION	FORM	TAG	FEATURES
    //(1:1:1:1)	bi	PP	PREFIX|bi+
    //(1:1:1:2)	somi	N	STEM|POS:N|LEM:{som|ROOT:smw|M|GEN
    //(1:1:2:1)	{ll~ahi	PN	STEM|POS:PN|LEM:{ll~ah|ROOT:Alh|GEN
    //(1:1:3:1)	{l	DET	PREFIX|Al+
    //(1:1:3:2)	r~aHoma`ni	ADJ	STEM|POS:ADJ|LEM:r~aHoma`n|ROOT:rHm|MS|GEN
    //(1:1:4:1)	{l	DET	PREFIX|Al+
    //(1:1:4:2)	r~aHiymi	ADJ	STEM|POS:ADJ|LEM:r~aHiym|ROOT:rHm|MS|GEN
    //(1:2:1:1)	{lo	DET	PREFIX|Al+
    //(1:2:1:2)	Hamodu	N	STEM|POS:N|LEM:Hamod|ROOT:Hmd|M|NOM
    //(1:2:2:1)	li	PP	PREFIX|l_PP+
    //(1:2:2:2)	l~ahi	PN	STEM|POS:PN|LEM:{ll~ah|ROOT:Alh|GEN
    //(1:2:3:1)	rab~i	N	STEM|POS:N|LEM:rab~|ROOT:rbb|M|GEN
    //(1:2:4:1)	{lo	DET	PREFIX|Al+
    //(1:2:4:2)	Ea`lamiyna	N	STEM|POS:N|LEM:Ea`lamiyn|ROOT:Elm|MP|GEN
    //(1:3:1:1)	{l	DET	PREFIX|Al+
    //(1:3:1:2)	r~aHoma`ni	ADJ	STEM|POS:ADJ|LEM:r~aHoma`n|ROOT:rHm|MS|GEN
    //(1:3:2:1)	{l	DET	PREFIX|Al+
    //(1:3:2:2)	r~aHiymi	ADJ	STEM|POS:ADJ|LEM:r~aHiym|ROOT:rHm|MS|GEN
    //(1:4:1:1)	ma`liki	N	STEM|POS:N|ACT_PCPL|LEM:ma`lik|ROOT:mlk|M|GEN
    //(1:4:2:1)	yawomi	N	STEM|POS:N|LEM:yawom|ROOT:ywm|M|GEN
    //(1:4:3:1)	{l	DET	PREFIX|Al+
    //(1:4:3:2)	d~iyni	N	STEM|POS:N|LEM:diyn|ROOT:dyn|M|GEN
    //(1:5:1:1)	<iy~aAka	PRON	STEM|POS:PRON|LEM:<iy~aA|2MS
    //(1:5:2:1)	naEobudu	V	STEM|POS:V|IMPF|LEM:Eabada|ROOT:Ebd|1P
    //(1:5:3:1)	wa	CONJ	PREFIX|w_CONJ+
    //(1:5:3:2)	<iy~aAka	PRON	STEM|POS:PRON|LEM:<iy~aA|2MS
    //(1:5:4:1)	nasotaEiynu	V	STEM|POS:V|IMPF|(X)|LEM:{sotaEiynu|ROOT:Ewn|1P
    //(1:6:1:1)	{hodi	V	STEM|POS:V|IMPV|LEM:hadaY|ROOT:hdy|2MS
    //(1:6:1:2)	naA	PRON	SUFFIX|+PRON:1P
    //(1:6:2:1)	{l	DET	PREFIX|Al+
    //(1:6:2:2)	S~ira`Ta	N	STEM|POS:N|LEM:Sira`T|ROOT:SrT|M|ACC
    //(1:6:3:1)	{lo	DET	PREFIX|Al+
    //(1:6:3:2)	musotaqiyma	ADJ	STEM|POS:ADJ|ACT_PCPL|(X)|LEM:m~usotaqiym|ROOT:qwm|M|ACC
    //(1:7:1:1)	Sira`Ta	N	STEM|POS:N|LEM:Sira`T|ROOT:SrT|M|ACC
    //(1:7:2:1)	{l~a*iyna	REL	STEM|POS:REL|LEM:{l~a*iY|MP
    //(1:7:3:1)	>anoEamo	V	STEM|POS:V|PERF|(IV)|LEM:>anoEama|ROOT:nEm|2MS
    //(1:7:3:2)	ta	PRON	SUFFIX|+PRON:2MS
    //(1:7:4:1)	Ealayo	PP	STEM|POS:PP|LEM:EalaY`
    //(1:7:4:2)	himo	PRON	SUFFIX|+PRON:3MP
    //(1:7:5:1)	gayori	N	STEM|POS:N|LEM:gayor|ROOT:gyr|M|GEN
    //(1:7:6:1)	{lo	DET	PREFIX|Al+
    //(1:7:6:2)	magoDuwbi	N	STEM|POS:N|PASS_PCPL|LEM:magoDuwb|ROOT:gDb|M|GEN
    //(1:7:7:1)	Ealayo	PP	STEM|POS:PP|LEM:EalaY`
    //(1:7:7:2)	himo	PRON	SUFFIX|+PRON:3MP
    //(1:7:8:1)	wa	CONJ	PREFIX|w_CONJ+
    //(1:7:8:2)	laA	NEG	STEM|POS:NEG|LEM:laA
    //(1:7:9:1)	{l	DET	PREFIX|Al+
    //(1:7:9:2)	D~aA^l~iyna	N	STEM|POS:N|ACT_PCPL|LEM:DaA^l~|ROOT:Dll|MP|GEN
    //...

    public class WordPart
    {
        private Word word = null;
        public Word Word
        {
            get { return word; }
        }

        public string Address
        {
            get
            {
                if (word != null)
                {
                    return (this.word.Address + ":" + number_in_word.ToString());
                }
                return "XXX:XXX:XXX:XXX";
            }
        }

        private int number_in_word = 0;
        public int NumberInWord
        {
            get { return number_in_word; }
        }

        // direct letter-by-letter transliteration scheme
        // http://www.qamus.org/transliteration.htm
        private string buckwalter = "";
        public string Buckwalter
        {
            get { return buckwalter; }
            set { buckwalter = value; } // to allow adding shadda on B or Bism for chapters 95 and 97
        }

        private string text = "";
        public string Text
        {
            get { return text; }
        }

        private string tag = "";
        public string Tag
        {
            get { return tag; }
        }

        private WordPartFeatures features = null;
        public WordPartFeatures Features
        {
            get { return features; }
        }

        public WordPart(Word word, int number_in_word, string buckwalter, string tag, List<string> features)
            : this(word, number_in_word, buckwalter, tag)
        {
            this.features = new WordPartFeatures(this, features);
        }
        public WordPart(Word word, int number_in_word, string buckwalter, string tag, WordPartFeatures features)
            : this(word, number_in_word, buckwalter, tag)
        {
            this.features = new WordPartFeatures(features);
        }
        public WordPart(Word word, int number_in_word, string buckwalter, string tag)
        {
            this.word = word;
            this.word.Parts.Add(this);
            this.number_in_word = number_in_word;
            this.buckwalter = buckwalter;
            this.text = buckwalter.ToArabic();
            this.tag = tag;
            this.features = null;
        }

        public override string ToString()
        {
            return ToTable();
        }
        public string ToTable()
        {
            StringBuilder str = new StringBuilder();
            str.Append(Address + "\t");
            str.Append(Text + "\t");
            str.Append(Buckwalter + "\t");
            str.Append(Tag + "\t");
            str.Append(Features.ToTable());
            return str.ToString();
        }
        public string ToArabic()
        {
            StringBuilder str = new StringBuilder();
            //str.Append(Address + "\t");
            str.Append(Text + "\t");
            //str.Append(WordPartFeatureDictionary.Arabic(Tag) + "\t");
            str.Append(Features.ToArabic());
            return str.ToString();
        }
        public string ToEnglish()
        {
            StringBuilder str = new StringBuilder();
            //str.Append(Address + "\t");
            str.Append(Buckwalter + "\t");
            //str.Append(WordPartFeatureDictionary.English(Tag) + "\t");
            str.Append(Features.ToEnglish());
            return str.ToString();
        }
        public string ToGrammar()
        {
            StringBuilder str = new StringBuilder();
            //str.Append(Address + "\t");
            str.Append(Text + "\t");
            //str.Append(WordPartFeatureDictionary.Arabic(Tag) + "\t");
            str.Append(Features.ToGrammar());
            return str.ToString();
        }
    }
}
