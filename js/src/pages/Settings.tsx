import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {styles} from "../utils/styles";
import {useNavigate} from "react-router-dom";

const Settings: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);
    const classes = styles();
    const history = useNavigate();
    const [apiKey, setApiKey] = useState<string | null>(context.apiKey);
    const [signedIn, setSignedIn] = useState<string | null>(context.googleAuth && context.googleAuth.isSignedIn.get());
    const [partition, setPartition] = useState<string | null>(context.partition);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true} className={classes.container}>
                {context.settings.requireApiKey !== "false" &&
                    <>
                        <Grid className={classes.cell} item md={2} sm={12} xs={12}>
                            <FormLabel className={classes.label}>API Key</FormLabel>
                        </Grid>
                        <Grid className={classes.cell} item md={10} sm={12} xs={12}>
                            <TextField id="apiKey" fullWidth={true} type="password" variant="outlined" value={apiKey}
                                       onChange={v => {
                                           setApiKey(v.target.value);
                                           localStorage.setItem("apiKey", v.target.value);
                                       }}/>
                        </Grid>
                    </>
                }
                <Grid className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Data Partition</FormLabel>
                </Grid>
                <Grid className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id="partition" fullWidth={true} variant="outlined" value={partition}
                               onChange={v => {
                                   setPartition(v.target.value);

                               }}/>
                    <span className={classes.helpText}>
                        <p>
                            The data partition defines what resources the web app has access to. All resources under the
                            default partition of "main" can be read, only resources in the current partition
                            can be edited or deleted, and new resources will be placed into the current partition.
                        </p>
                        <p>
                            Set the data partition to "main" to work in default partition.
                        </p>
                    </span>
                </Grid>
                <Grid className={classes.cell} item md={2} sm={12} xs={12}>
                    <FormLabel className={classes.label}>Developer Login</FormLabel>
                </Grid>
                <Grid className={classes.cell} item md={10} sm={12} xs={12}>
                    {!context.googleAuth &&
                        <Button variant={"outlined"} disabled={true}>Loading...</Button>}
                    {context.googleAuth &&
                        <span>
                            {signedIn
                                ? <Button variant={"outlined"} onClick={_ => logout()}>Logout</Button>
                                : <Button variant={"outlined"} onClick={_ => login()}>Login</Button>}
                        </span>
                    }
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <Button variant={"outlined"} onClick={_ => saveSettings()}>Save Settings</Button>
                </Grid>
            </Grid>
        </>
    );

    function login() {
        if (context.googleAuth && !context.googleAuth.isSignedIn.get()) {
            context.googleAuth.signIn()
                .then(() => {
                    setSignedIn(context.googleAuth && context.googleAuth.isSignedIn.get())
                });
        }
    }

    function logout() {
        if (context.googleAuth && context.googleAuth.isSignedIn.get()) {
            context.googleAuth.signOut().then(() => {
                setSignedIn(context.googleAuth && context.googleAuth.isSignedIn.get())
            });
        }
    }

    function saveSettings() {
        const fixedPartition = partition ? partition.trim() : "";
        const fixedApiKey = apiKey ? apiKey.trim() : "";
        localStorage.setItem("partition", fixedPartition);
        localStorage.setItem("apiKey", fixedApiKey);
        context.setPartition(fixedPartition);
        context.setApiKey(fixedApiKey);
        history('/index.html');
    }
}


export default Settings;