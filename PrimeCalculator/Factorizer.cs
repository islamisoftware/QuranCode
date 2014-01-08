using System;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace PrimeCalculator
{
    class Factorizer
    {
        private Form m_form;      // gui object to notify with BeginInvoke
        private long m_number;
        private bool? m_is_prime;
        private ArrayList m_factors;
        private bool m_is_cancel;
        private DateTime m_start;
        private TimeSpan m_duration;

        public Factorizer(Form form, long number)
        {
            m_form = form;
            m_number = number;
            m_is_prime = null;
            m_factors = new ArrayList();
            m_is_cancel = false;
            m_start = DateTime.Now;
            m_duration = TimeSpan.Zero;
        }


        public long Number
        {
            get { return m_number; }
        }

        public bool? IsPrime
        {
            get { return m_is_prime; }
            set { m_is_prime = value; }
        }

        public ArrayList Factors
        {
            get { return m_factors; }
        }

        public TimeSpan Duration
        {
            get { return m_duration; }
        }

        public bool Cancel
        {
            get { return m_is_cancel; }
            set { m_is_cancel = value; }
        }

        public void Run()
        {
            if (m_form != null)
            {
                // Hook update delegates to the methods to be invoked
                MainForm.UpdateProgressBar UpdateProgressBarDelegate = ((MainForm)m_form).UpdateProgressBarMethod;
                MainForm.UpdateFactorsTextBox UpdateFactorsTextBoxDelegate = ((MainForm)m_form).UpdateFactorsTextBoxMethod;

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                m_start = DateTime.Now;
                ///////////////////////////////////////////////////////////////////////////////////////////////////

                m_is_prime = true; // innocent until proven guilty :)

                m_is_cancel = false;
                long batch_size = 300000L; // to keep digital clock running

                string prime_factors_str = string.Empty;

                if (m_factors != null)
                {
                    m_factors.Clear();

                    try
                    {
                        if (m_number < 2L)
                        {
                            m_is_prime = null;
                        }
                        else if (m_number == 2L)
                        {
                            m_is_prime = true;
                            m_factors.Add(m_number);

                            // notify form of new prime factor
                            prime_factors_str += m_number.ToString();
                            m_form.BeginInvoke(UpdateFactorsTextBoxDelegate, new object[] { prime_factors_str });
                            Thread.Sleep(((MainForm)m_form).SleepTime);
                        }
                        else
                        {
                            // test if number is prime
                            // if yes add it to factors, number /= p, loop until 1
                            long number = m_number;
                            while (number > 1L)
                            {
                                if ((number % 2L) == 0L) // if even number
                                {
                                    m_is_prime = false;
                                    m_factors.Add(2L);
                                    number /= 2L;

                                    // notify form of new prime factor
                                    prime_factors_str += ((prime_factors_str == string.Empty) ? "" : " ") + "2" + ((number > 1L) ? " *" : "");
                                    m_form.BeginInvoke(UpdateFactorsTextBoxDelegate, new object[] { prime_factors_str });
                                    Thread.Sleep(((MainForm)m_form).SleepTime);
                                }
                                else // trial divide by all odds upto sqrt(number)
                                {
                                    long max = (long)(Math.Sqrt(number)) + 1L;	// extra 1 for double calculation errors

                                    bool is_prime_factor_found = false;
                                    for (long p = 3L; p <= max; p += 2L)		// should only use primes, but odds will do for 19-digit-max longs
                                    {
                                        // notify form every batch_size iteration
                                        if ((p % batch_size) == 1L) // we're using odd
                                        {
                                            m_duration = DateTime.Now - m_start;
                                            m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { ((int)((long)(p * 100L) / max)) });
                                            Thread.Sleep(((MainForm)m_form).SleepTime);
                                        }

                                        // if divisible then
                                        if ((number % p) == 0L)
                                        {
                                            m_is_prime = false;
                                            m_factors.Add(number);
                                            number /= p;

                                            // notify form every new prime factor
                                            prime_factors_str += ((prime_factors_str == string.Empty) ? "" : " ") + p + ((number > 1L) ? " *" : "");
                                            m_form.BeginInvoke(UpdateFactorsTextBoxDelegate, new object[] { prime_factors_str });
                                            Thread.Sleep(((MainForm)m_form).SleepTime);

                                            is_prime_factor_found = true;
                                            break; // leave for loop
                                        }

                                        if (m_is_cancel)
                                        {
                                            m_is_prime = null;
                                            break; // leave for loop
                                        }
                                    } // next p

                                    // if after all trials and no prime factor found
                                    // then number must be the last prime factor
                                    if (!is_prime_factor_found)
                                    {
                                        m_factors.Add(number);

                                        // notify form every new prime factor
                                        prime_factors_str += ((prime_factors_str == string.Empty) ? "" : " ") + number.ToString();
                                        m_form.BeginInvoke(UpdateFactorsTextBoxDelegate, new object[] { prime_factors_str });
                                        Thread.Sleep(((MainForm)m_form).SleepTime);

                                        number /= number;
                                        break; // leave while loop
                                    }

                                }

                                if (m_is_cancel)
                                {
                                    m_is_prime = null;
                                    break; // leave while loop
                                }
                            } // while
                        }

                        ///////////////////////////////////////////////////////////////////////////////////////////////////
                        m_duration = DateTime.Now - m_start;
                        ///////////////////////////////////////////////////////////////////////////////////////////////////

                        // send -1  --> process was cancelled
                        // send 100 --> process has finished naturally
                        m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { (int)((m_is_cancel) ? -1 : 100) });
                        Thread.Sleep(((MainForm)m_form).SleepTime);
                    }
                    catch
                    {
                        // cancel programmatically
                        m_is_cancel = true;
                        // send -1  --> process was cancelled
                        if (m_form.Handle != null)
                        {
                            m_form.BeginInvoke(UpdateProgressBarDelegate, new object[] { -1 });
                            Thread.Sleep(((MainForm)m_form).SleepTime);
                        }
                        ///////////////////////////////////////////////////////////////////////////////////////////////////
                        m_duration = DateTime.Now - m_start;
                        ///////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                }
            }
        }
    }
}
