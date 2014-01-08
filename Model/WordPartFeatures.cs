using System;
using System.Text;
using System.Collections.Generic;

namespace Model
{
    public class WordPartFeatures
    {
        private WordPart word_part = null;
        public WordPart WordPart
        {
            get { return word_part; }
        }

        private string type = "";
        public string Type
        {
            get { return type; }
        }

        private string position = "";
        public string Position
        {
            get { return position; }
        }

        private string attribute = "";
        public string Attribute
        {
            get { return attribute; }
        }

        private string qualifier = "";
        public string Qualifier
        {
            get { return qualifier; }
        }

        private string person_degree = "";
        public string PersonDegree
        {
            get { return person_degree; }
        }

        private string person_gender = "";
        public string PersonGender
        {
            get { return person_gender; }
        }

        private string person_number = "";
        public string PersonNumber
        {
            get { return person_number; }
        }

        private string mood = "";
        public string Mood
        {
            get { return mood; }
        }

        private string lemma = "";
        public string Lemma
        {
            get { return lemma; }
        }

        private string root = "";
        public string Root
        {
            get { return root; }
        }

        private string special_group = "";
        public string SpecialGroup
        {
            get { return special_group; }
        }

        // instance constructor
        //Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber	Mood	Lemma	Root	SpecialGroup
        public WordPartFeatures(WordPart word_part, List<string> features)
        {
            this.word_part = word_part;

            if (features != null)
            {
                if (features.Count > 1)
                {
                    this.type = features[0];

                    switch (this.type)
                    {
                        case "PREFIX":
                            {
                                //Al+
                                //bi+
                                //bip+
                                //ka+
                                //ta+
                                //sa+
                                //ya+
                                //ha+
                                //A_INTG+
                                //A_EQ+"
                                //b_PART+
                                //f_REM+
                                //f_CONJ+
                                //f_RSLT+
                                //f_SUP+
                                //f_CAUS+
                                //l_PP+
                                //l_EMPH+
                                //l_PRP+
                                //l_IMPV+
                                //w_CONJ+
                                //w_REM+
                                //w_CIRC+
                                //w_SUP+
                                //w_PP+
                                //w_COM+
                                if (features.Count == 2)
                                {
                                    this.position = features[1];
                                }
                                else
                                {
                                    throw new Exception("WordPartFeatures: Invalide PREFIX at word part " + word_part.Address);
                                }
                            }
                            break;
                        case "STEM":
                            {
                                for (int i = 1; i < features.Count; i++)
                                {
                                    string[] parts = features[i].Split(':');

                                    if (parts.Length == 2)
                                    {
                                        if (parts[0] == "POS")
                                        {
                                            //INL
                                            //N
                                            //PN
                                            //V
                                            //ADJ
                                            //IMPN
                                            //AC
                                            //AMD
                                            //ANS
                                            //AVR
                                            //CERT
                                            //COND
                                            //EXH
                                            //EXL
                                            //EXP
                                            //FUT
                                            //INC
                                            //INT
                                            //INTG
                                            //NEG
                                            //PREV
                                            //PRO
                                            //RES
                                            //RET
                                            //SUP
                                            //SUR
                                            //PP
                                            //CONJ
                                            //SUB
                                            //EQ
                                            //REM
                                            //CIRC
                                            //COM
                                            //RSLT
                                            //CAUS
                                            //EMPH
                                            //PRP
                                            //IMPV
                                            //PRON
                                            //DEM
                                            //REL
                                            //T
                                            //LOC
                                            this.position = parts[1];
                                        }
                                        else if (parts[0] == "LEM")
                                        {
                                            this.lemma = parts[1];
                                        }
                                        else if (parts[0] == "ROOT")
                                        {
                                            this.root = parts[1];
                                        }
                                        else if (parts[0] == "SP")
                                        {
                                            this.special_group = parts[1];
                                        }
                                        else if (parts[0] == "MOOD")
                                        {
                                            this.mood = parts[1];
                                        }
                                        else
                                        {
                                            throw new Exception("WordPartFeatures: Invalide STEM at word part " + word_part.Address);
                                        }
                                    }
                                    else if (parts.Length == 1) // attribute or qualifier
                                    {
                                        switch (parts[0])
                                        {
                                            case "1":
                                            case "2":
                                            case "3":
                                                this.person_degree = parts[0][0].ToString();
                                                break;
                                            case "M":
                                            case "F":
                                                this.person_gender = parts[0][0].ToString();
                                                break;
                                            case "S":
                                            case "D":
                                            case "P":
                                                this.person_number = parts[0][0].ToString();
                                                break;
                                            case "1M":
                                            case "2M":
                                            case "3M":
                                            case "1F":
                                            case "2F":
                                            case "3F":
                                                this.person_degree = parts[0][0].ToString();
                                                this.person_gender = parts[0][1].ToString();
                                                break;
                                            case "1S":
                                            case "2S":
                                            case "3S":
                                            case "1D":
                                            case "2D":
                                            case "3D":
                                            case "1P":
                                            case "2P":
                                            case "3P":
                                                this.person_degree = parts[0][0].ToString();
                                                this.person_number = parts[0][1].ToString();
                                                break;
                                            case "MS":
                                            case "FS":
                                            case "MD":
                                            case "FD":
                                            case "MP":
                                            case "FP":
                                                this.person_gender = parts[0][0].ToString();
                                                this.person_number = parts[0][1].ToString();
                                                break;
                                            case "1MS":
                                            case "2MS":
                                            case "3MS":
                                            case "1FS":
                                            case "2FS":
                                            case "3FS":
                                            case "1MD":
                                            case "2MD":
                                            case "3MD":
                                            case "1FD":
                                            case "2FD":
                                            case "3FD":
                                            case "1MP":
                                            case "2MP":
                                            case "3MP":
                                            case "1FP":
                                            case "2FP":
                                            case "3FP":
                                                this.person_degree = parts[0][0].ToString();
                                                this.person_gender = parts[0][1].ToString();
                                                this.person_number = parts[0][2].ToString();
                                                break;
                                            default:
                                                if (this.attribute.Length == 0)
                                                {
                                                    //VN
                                                    //ACT_PCPL
                                                    //PASS_PCPL
                                                    //NOM
                                                    //ACC
                                                    //GEN
                                                    //DEF
                                                    //INDEF
                                                    //PERF
                                                    //IMPF
                                                    //IMPV
                                                    this.attribute = parts[0];
                                                }
                                                else
                                                {
                                                    //NOM
                                                    //ACC
                                                    //ACT
                                                    //PASS
                                                    //(I)
                                                    //(II)
                                                    //(III)
                                                    //(IV)
                                                    //(V)
                                                    //(VI)
                                                    //(VII)
                                                    //(VIII)
                                                    //(IX)
                                                    //(X)
                                                    //(XI)
                                                    //(XII)
                                                    this.qualifier = parts[0];
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            break;
                        case "SUFFIX":
                            {
                                if (features.Count == 2)
                                {
                                    //SUFFIX|+n_EMPH
                                    //SUFFIX|+VOC
                                    //SUFFIX|+l_PP
                                    //SUFFIX|+PRON:2MP
                                    //SUFFIX|+A_SILENT

                                    string[] parts = features[1].Split(':');

                                    if (parts.Length == 1)
                                    {
                                        this.position = parts[0];
                                    }
                                    else if (parts.Length == 2)
                                    {
                                        if (parts[0] == "+PRON")
                                        {
                                            this.position = parts[0];

                                            if (parts[1].Length == 3)
                                            {
                                                this.person_degree = parts[1][0].ToString();
                                                this.person_gender = parts[1][1].ToString();
                                                this.person_number = parts[1][2].ToString();
                                            }
                                            else if (parts[1].Length == 2)
                                            {
                                                if ((parts[1][0] == '1') || (parts[1][0] == '2') || (parts[1][0] == '3'))
                                                {
                                                    this.person_degree = parts[1][0].ToString();
                                                }
                                                else if ((parts[1][0] == 'M') || (parts[1][0] == 'F'))
                                                {
                                                    this.person_gender = parts[1][0].ToString();
                                                }
                                                else if ((parts[1][0] == 'S') || (parts[1][0] == 'D') || (parts[1][0] == 'P'))
                                                {
                                                    this.person_number = parts[1][0].ToString();
                                                }

                                                if ((parts[1][1] == 'M') || (parts[1][1] == 'F'))
                                                {
                                                    this.person_gender = parts[1][1].ToString();
                                                }
                                                else if ((parts[1][1] == 'S') || (parts[1][1] == 'D') || (parts[1][1] == 'P'))
                                                {
                                                    this.person_number = parts[1][1].ToString();
                                                }
                                            }
                                            else if (parts[1].Length == 1)
                                            {
                                                if ((parts[1][0] == '1') || (parts[1][0] == '2') || (parts[1][0] == '3'))
                                                {
                                                    this.person_degree = parts[1][0].ToString();
                                                }
                                                else if ((parts[1][0] == 'M') || (parts[1][0] == 'F'))
                                                {
                                                    this.person_gender = parts[1][0].ToString();
                                                }
                                                else if ((parts[1][0] == 'S') || (parts[1][0] == 'D') || (parts[1][0] == 'P'))
                                                {
                                                    this.person_number = parts[1][0].ToString();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("WordPartFeatures: SUFFIX|+PRON expected at word part " + word_part.Address);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("WordPartFeatures: Invalide SUFFIX at word part " + word_part.Address);
                                    }
                                }
                                else
                                {
                                    throw new Exception("WordPartFeatures: Invalide SUFFIX at word part " + word_part.Address);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // copy constructor
        //Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber	Mood	Lemma	Root	SpecialGroup
        public WordPartFeatures(WordPartFeatures features)
        {
            this.word_part = features.WordPart;
            this.type = features.Type;
            this.position = features.Position;
            this.attribute = features.Attribute;
            this.qualifier = features.Qualifier;
            this.person_degree = features.PersonDegree;
            this.person_gender = features.PersonGender;
            this.person_number = features.PersonNumber;
            this.mood = features.Mood;
            this.lemma = features.Lemma;
            this.root = features.Root;
            this.special_group = features.SpecialGroup;
        }

        //Type	Position	Attribute	Qualifier	PersonDegree	PersonGender	PersonNumber    Mood	Lemma	Root	SpecialGroup
        public override string ToString()
        {
            return ToTable();
        }
        public string ToTable()
        {
            StringBuilder str = new StringBuilder();
            str.Append(Type + "\t");
            str.Append(Position + "\t");
            str.Append(Attribute + "\t");
            str.Append(Qualifier + "\t");
            str.Append(PersonDegree + "\t");
            str.Append(PersonGender + "\t");
            str.Append(PersonNumber + "\t");
            str.Append(Mood + "\t");
            str.Append(Lemma.ToArabic() + "\t");
            str.Append(Root.ToArabic() + "\t");
            str.Append(SpecialGroup.ToArabic());
            return str.ToString();
        }
        public string ToArabic()
        {
            StringBuilder str = new StringBuilder();
            //if (Type.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Type) + "\t");
            if (Position.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Position) + "\t");
            if (Attribute.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Attribute) + "\t");
            if (Qualifier.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Qualifier) + "\t");
            if (PersonDegree.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonDegree) + "\t");
            if (PersonGender.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonGender) + "\t");
            if (PersonNumber.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonNumber) + "\t");
            if (Mood.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Mood) + "\t");
            //if (Lemma.Length > 0) str.Append(Lemma.ToArabic() + "\t");
            //if (Root.Length > 0) str.Append(Root.ToArabic() + "\t");
            //if (SpecialGroup.Length > 0) str.Append(SpecialGroup.ToArabic() + "\t");
            if (str.Length > 2)
            {
                str.Remove(str.Length - 1, 1);
            }
            return str.ToString();
        }
        public string ToEnglish()
        {
            StringBuilder str = new StringBuilder();
            //if (Type.Length > 0) str.Append(WordPartFeatureDictionary.English(Type) + "\t");
            if (Position.Length > 0) str.Append(WordPartFeatureDictionary.English(Position) + "\t");
            if (Attribute.Length > 0) str.Append(WordPartFeatureDictionary.English(Attribute) + "\t");
            if (Qualifier.Length > 0) str.Append(WordPartFeatureDictionary.English(Qualifier) + "\t");
            if (PersonDegree.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonDegree) + "\t");
            if (PersonGender.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonGender) + "\t");
            if (PersonNumber.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonNumber) + "\t");
            if (Mood.Length > 0) str.Append(WordPartFeatureDictionary.English(Mood) + "\t");
            //if (Lemma.Length > 0) str.Append(Lemma + "\t");
            //if (Root.Length > 0) str.Append(Root + "\t");
            //if (SpecialGroup.Length > 0) str.Append(SpecialGroup + "\t");
            if (str.Length > 2)
            {
                str.Remove(str.Length - 1, 1);
            }
            return str.ToString();
        }
        public string ToGrammar()
        {
            StringBuilder str = new StringBuilder();
            //if (Type.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Type) + " ");
            //if (Type.Length > 0) str.Append(WordPartFeatureDictionary.English(Type) + "\t");
            if (Position.Length > 0) str.Append(WordPartFeatureDictionary.English(Position) + " ");
            if (Position.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Position) + "\t");
            if (Attribute.Length > 0) str.Append(WordPartFeatureDictionary.English(Attribute) + " ");
            if (Attribute.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Attribute) + "\t");
            if (Qualifier.Length > 0) str.Append(WordPartFeatureDictionary.English(Qualifier) + " ");
            if (Qualifier.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Qualifier) + "\t");
            if (PersonDegree.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonDegree) + " ");
            if (PersonDegree.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonDegree) + "\t");
            if (PersonGender.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonGender) + " ");
            if (PersonGender.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonGender) + "\t");
            if (PersonNumber.Length > 0) str.Append(WordPartFeatureDictionary.English(PersonNumber) + " ");
            if (PersonNumber.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(PersonNumber) + "\t");
            if (Mood.Length > 0) str.Append(WordPartFeatureDictionary.English(Mood) + " ");
            if (Mood.Length > 0) str.Append(WordPartFeatureDictionary.Arabic(Mood) + "\t");
            //if (Lemma.Length > 0) str.Append(Lemma.ToArabic() + "\t");
            //if (Root.Length > 0) str.Append(Root.ToArabic() + "\t");
            //if (SpecialGroup.Length > 0) str.Append(SpecialGroup.ToArabic() + "\t");
            if (str.Length > 2)
            {
                str.Remove(str.Length - 1, 1);
            }
            return str.ToString();
        }
    }
}
