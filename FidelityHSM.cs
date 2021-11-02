using System;
using System.Collections.Generic;
using Common.Logging;
using Veneka.Indigo.Integration.DAL;
using Veneka.Indigo.Integration.Objects;
using Veneka.Indigo.Integration.Common;
using System.Diagnostics;

using Veneka.Indigo.Integration.Config;
using System.IO;

namespace Veneka.Indigo.Integration.Fidelity
{
    //[IntegrationExport("FidelityHSM", "F624756E-39A8-4013-9C45-A18AC789D53B", typeof(IHardwareSecurityModule))]
    //public class FidelityHSM : IHardwareSecurityModule
    //{
    //    private static readonly ILog _hsmLog = LogManager.GetLogger(General.HSM_LOGGER);

    //    public string SQLConnectionString { get; set; }

    //    public DirectoryInfo IntegrationFolder
    //    {
    //        get;set;
    //    }

    //    public IDataSource DataSource { get; set; }

    //    public bool GenerateCVV(ref List<CardObject> cardObjects, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateCardEncryptionData(ref List<CardObject> cardObjects, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GeneratePVV(ref List<CardObject> cardObjects, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PrintPins(ref List<CardObject> cardObjects, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateRandomKey(int issuerId, string tmk, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out TerminalSessionKey randomKeys)
    //    {
    //        responseMessage = String.Empty;
    //        randomKeys = new TerminalSessionKey(String.Empty, String.Empty);

    //        //ParametersDAL pDAL = new ParametersDAL(this.SQLConnectionString);

    //        //var parm = pDAL.GetParameter(issuerId, 2, 0, null, auditUserId, auditWorkStation);

    //        //if (parm == null)
    //        //    throw new ArgumentNullException("HSM Interface parameters not set for issuer");

    //        ThalesConfig thalesConfig;

    //        if (config is ThalesConfig)
    //            thalesConfig = (ThalesConfig)config;
    //        else
    //            throw new ArgumentException("HSM config parameters must be for Thales.");

    //        Veneka.Module.ThalesHSM.HSMInterfaceManager hsmInterface = new Veneka.Module.ThalesHSM.HSMInterfaceManager(thalesConfig.HeaderLength, thalesConfig.Address, thalesConfig.Port, thalesConfig.Timeout.Value);

    //        try
    //        {
    //            var keys = hsmInterface.GenerateRandomKey(tmk,
    //                                                        ThalesCommand.KeyScheme.KeySchemeTag.X,
    //                                                        ThalesCommand.KeyScheme.KeySchemeTag.U,
    //                                                        null);

    //            //Remove the first character from they key, this is because thales includes the keyscheme in the first character
    //            //which is not needed.
    //            randomKeys = new TerminalSessionKey(keys.RandomKey, keys.RandomKey_LMK);

    //            return true;
    //        }
    //        catch (Veneka.Module.ThalesHSM.Exceptions.ThalesException tex)
    //        {
    //            _hsmLog.Error(tex);
    //            responseMessage = tex.Message;
    //        }

    //        return false;
    //    }

    //    public bool ReadTrackData(int issuerId, string keyUnderLMK, string trackData, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out Track2 track2)
    //    {
    //        responseMessage = String.Empty;
    //        track2 = null;

    //        //ParametersDAL pDAL = new ParametersDAL(this.SQLConnectionString);
    //        //TerminalDAL termDAL = new TerminalDAL(this.SQLConnectionString);

    //        //var parm = pDAL.GetParameter(issuerId, 2, 0, null, auditUserId, auditWorkStation);

    //        //if (parm == null)
    //        //    throw new ArgumentNullException("HSM Interface parameters not set for issuer");

    //        ThalesConfig thalesConfig;

    //        if (config is ThalesConfig)
    //            thalesConfig = (ThalesConfig)config;
    //        else
    //            throw new ArgumentException("HSM config parameters must be for Thales.");

    //        var zoneKey = DataSource.TerminalDAL.GetZoneMasterKey(issuerId, auditUserId, auditWorkStation);

    //        if (zoneKey == null)
    //            throw new ArgumentNullException("Zone Key parameters not set for issuer");

    //        HSMInterfaceManager hsmInterface = new HSMInterfaceManager(thalesConfig.HeaderLength, thalesConfig.Address, thalesConfig.Port, thalesConfig.Timeout.Value);

    //        try
    //        {
    //            track2 = DecryptTrackData(issuerId, keyUnderLMK, trackData, zoneKey, hsmInterface);

    //            return true;
    //        }
    //        catch (Veneka.Module.ThalesHSM.Exceptions.ThalesException tex)
    //        {
    //            _hsmLog.Error(tex);
    //            responseMessage = tex.Message;
    //        }

    //        return false;
    //    }

    //    public bool PinFromPinBlock(int issuerId, string keyUnderLMK, string pinBlock, string trackData, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pin)
    //    {
    //        responseMessage = String.Empty;
    //        pin = String.Empty;

    //        //ParametersDAL pDAL = new ParametersDAL(this.SQLConnectionString);
    //        //ProductDAL prodDAL = new ProductDAL(this.SQLConnectionString);
    //        //TerminalDAL termDAL = new TerminalDAL(this.SQLConnectionString);

    //        //var parm = pDAL.GetParameter(issuerId, 2, 0, null, auditUserId, auditWorkStation);

    //        //if (parm == null)
    //        //    throw new ArgumentNullException("HSM Interface parameters not set for issuer");
    //        ThalesConfig thalesConfig;

    //        if (config is ThalesConfig)
    //            thalesConfig = (ThalesConfig)config;
    //        else
    //            throw new ArgumentException("HSM config parameters must be for Thales.");

    //        var zoneKey = DataSource.TerminalDAL.GetZoneMasterKey(issuerId, auditUserId, auditWorkStation);

    //        if (zoneKey == null)
    //            throw new ArgumentNullException("Zone Key parameters not set for issuer");

    //        HSMInterfaceManager hsmInterface = new HSMInterfaceManager(thalesConfig.HeaderLength, thalesConfig.Address, thalesConfig.Port, thalesConfig.Timeout.Value);

    //        var track2 = DecryptTrackData(issuerId, keyUnderLMK, trackData, zoneKey, hsmInterface);

    //        WriteToLog("TRACK PAN=" + track2.PAN);
    //        var product = DataSource.ProductDAL.FindBestMatch(issuerId, track2.PAN, true, auditUserId, auditWorkStation);

    //        if (product == null)
    //            throw new Exception("Could not find product matching the suppled card details.");

    //        try
    //        {
    //            var pinLMK = hsmInterface.PinFromTPKToLMK(keyUnderLMK, pinBlock, ThalesCommand.PinBlockFormats.ISO9564_1_ANSIX9_8_FORMAT_0, track2.PAN, null);

    //            //find card details based on pan
    //            //string pvk = "U05710E1A5CA2AB3035B6B8CC475A1F0A";
    //            //int pvki = 1;                

    //            string cardPvv = track2.PVV.Substring(1);

    //            int pvvNew = int.Parse(hsmInterface.GenerateVisaPVV(pinLMK, product.PVK, product.PVKI, track2.PAN, null));

    //            //Now calculate the offset.
    //            int Offset = (10000 + (pvvNew - int.Parse(cardPvv))) % 10000;
    //            pin = Offset.ToString();

    //            WriteToLog(track2.PAN, product.PVK, product.PVKI.ToString(), pinLMK, pvvNew.ToString(), track2.PVV, cardPvv, Offset.ToString());

    //            return true;
    //        }
    //        catch (Veneka.Module.ThalesHSM.Exceptions.ThalesException tex)
    //        {
    //            _hsmLog.Error(tex);
    //            responseMessage = tex.Message;
    //        }

    //        return false;
    //    }

    //    private Track2 DecryptTrackData(int issuerId, string keyUnderLMK, string trackData, ZoneMasterKey zmk, HSMInterfaceManager hsmInterface)
    //    {
    //        //TODO Fetch zmk for issuer.
    //        //string zmk = "X7B4D63B3B1F8EEACCE014365BAFD7E4D";
    //        //string zmkFinalKey = "45B325F268320E31FBF14A040D6E6458";            

    //        var zmkKey = hsmInterface.TranslateKeyUnderLMKToZMKCommand(zmk.Zone, keyUnderLMK,
    //                                                                        ThalesCommand.KeyScheme.KeySchemeTag.X,
    //                                                                        ThalesCommand.KeyCheckValue.Type.KCV6H,
    //                                                                        null);

    //        string keyUnderZMK = zmkKey.Key;

    //        //Decrypt keyUnderZMK to get random key
    //        var randomKey = Cryptography.TripleDes.DecryptTripleDES(zmk.Final, keyUnderZMK.Substring(1));

    //        //Now decrypt the track2 data
    //        var clearTrackData = Cryptography.TripleDes.DecryptTripleDES(randomKey, trackData);

    //        if (clearTrackData.Length > 48 && Cryptography.Utility.IsHex(clearTrackData))
    //            clearTrackData = Cryptography.Utility.HexToString(clearTrackData);

    //        WriteToLog("TRACKDATA=" + clearTrackData);

    //        return new Track2(clearTrackData);
    //    }

    //    [Conditional("DEBUG")]
    //    private void WriteToLog(string message)
    //    {
    //        if (_hsmLog.IsDebugEnabled)
    //            _hsmLog.Debug(message);
    //    }

    //    [Conditional("DEBUG")]
    //    private void WriteToLog(string pan, string pvk, string pvki, string PIN_lmk, string pvvNew, string pvvCard, string pvvCard2, string pvvOffset)
    //    {
    //        if (_hsmLog.IsDebugEnabled)
    //        {
    //            _hsmLog.Debug("PAN          : " + pan);
    //            _hsmLog.Debug("PVK          : " + pvk);
    //            _hsmLog.Debug("PVKI         : " + pvki);
    //            _hsmLog.Debug("PIN_lmk      : " + PIN_lmk);
    //            _hsmLog.Debug("PVV NEW      : " + pvvNew);
    //            _hsmLog.Debug("PVV CARD(org): " + pvvCard);
    //            _hsmLog.Debug("PVV CARD(cal): " + pvvCard2);
    //            _hsmLog.Debug("PVV OFFSET : " + pvvOffset);
    //        }
    //    }

    //    public bool DecryptFields(ZoneMasterKey zmk, TerminalSessionKey tpk, string operatorCode, string pinBlock, string trackData, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out DecryptedFields decryptedFields)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateIBMPVV(int issuerId, string pvk, string pin, Track2 track2, string decimalisationTable, string pinValidationData, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pvv)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateVisaPVV(int issuerId, string pvk, string pvki, string pin, Track2 track2, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pvv)
    //    {
    //        throw new NotImplementedException();
    //    }

     
    //    public bool DecryptFields(ZoneMasterKey zmk, TerminalSessionKey tpk, Product product, Track2 track2, string operatorCode, string pinBlock, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out DecryptedFields decryptedFields)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool DecryptFields(ZoneMasterKey zmk, TerminalSessionKey tpk, Product product, TerminalCardData termCardData, string operatorCode, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out DecryptedFields decryptedFields)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateIBMPVV(int issuerId, Product product, DecryptedFields decryptedFields, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pvv)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool GenerateVisaPVV(int issuerId, Product product, DecryptedFields decryptedFields, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pvv)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool PinFromPinBlock(int issuerId, Product product, DecryptedFields decryptedFields, string deviceId, IConfig config, int languageId, long auditUserId, string auditWorkStation, out string responseMessage, out string pin)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
