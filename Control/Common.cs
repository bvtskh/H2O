using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMC_Hydroelectricity.Control
{
    public static class Common
    {
        public static void StartFormLoading()
        {
            FormLoading formLoading = new FormLoading();
            var formList = Application.OpenForms.Cast<Form>();
            var formActive = formList.Where(w => w.Name == "FormLoading").ToList();
            if (formActive.Count() == 0)
            {
                Thread thread = new Thread(() =>
                {
                    formLoading.ShowDialog();
                });
                thread.Start();
            }

        }
        public static void CloseFormLoading()
        {
            var formList = Application.OpenForms.Cast<Form>();
            var formLoad = formList.Where(w => w.Name == "FormLoading").FirstOrDefault();
            if (formLoad != null)
            {
                formLoad.Invoke(new MethodInvoker(delegate ()
                {
                    formLoad.Close();
                }));
            }
        }

    }
}
