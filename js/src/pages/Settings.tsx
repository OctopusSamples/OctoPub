import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {styles} from "../utils/styles";
import {useNavigate} from "react-router-dom";
import {setLoginBranch} from "../utils/path";
import {clearAccessToken, getAccessToken} from "../utils/security";

const Settings: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);
    const classes = styles();
    const history = useNavigate();
    const [partition, setPartition] = useState<string | null>(context.partition);

    const accessToken = getAccessToken(context.settings.aws.jwk);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true} className={classes.container}>
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
                        {accessToken
                            ? <Button variant={"outlined"} onClick={_ => logout()}>Logout</Button>
                            : <Button variant={"outlined"} onClick={_ => login()}>Login</Button>}
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
        setLoginBranch();
        window.location.href = context.settings.aws.cognitoLogin;
    }

    function logout() {
        clearAccessToken();
    }

    function saveSettings() {
        const fixedPartition = partition ? partition.trim() : "";
        localStorage.setItem("partition", fixedPartition);
        context.setPartition(fixedPartition);
        history('/index.html');
    }
}


export default Settings;