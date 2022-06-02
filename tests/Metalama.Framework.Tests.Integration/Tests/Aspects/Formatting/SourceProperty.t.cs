    [TestAspect]
    public class Target
    {
        public int _field;

        public int GetAutoProperty { get; }
        
        public int InitAutoProperty { get; init; }

        public int AutoProperty
        {
            get
            {
                return this.AutoProperty_Source;
            }

            set
            {
                if (value != this.AutoProperty_Source)
                {
                    this.AutoProperty_Source = value;
                }
            }
        }

        private int AutoProperty_Source { get; set; }

        public int Property
        {
            get
            {
                return this.Property_Source;
            }
            set
            {
                if (value != this.Property_Source)
                {
                    this.Property_Source = value;
                }
            }
        }

        private int Property_Source { get => _field; set => _field = value; }
    }