import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Button, FormLabel, Grid, TextField} from "@material-ui/core";
import {AppContext} from "../App";
import {styles} from "../utils/styles";
import {useNavigate} from "react-router-dom";
import {getAccessToken, login, logout} from "../utils/security";

const Settings: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);
    const classes = styles();
    const history = useNavigate();
    const [partition, setPartition] = useState<string | null>(context.partition);

    const accessToken = getAccessToken();

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
                            : <Button variant={"outlined"} onClick={_ => login(context.settings.aws.cognitoLogin)}>Login</Button>}
                    <span className={classes.helpText}>
                        <p>
                            If your account is part of the "Developers" group, you will be granted access to create,
                            modify, and delete books.
                        </p>
                        <p>
                            You must also log in to the "Developers" group to have any branching rules applied.
                        </p>
                        <p>
                            Your login is valid for an hour, after which the token expires and you must login again.
                        </p>
                    </span>
                </Grid>
                <Grid container={true} className={classes.cell} item md={2} sm={12} xs={12}>

                </Grid>
                <Grid container={true} className={classes.cell} item md={10} sm={12} xs={12}>
                    <Button variant={"outlined"} onClick={_ => saveSettings()}>Save Settings</Button>
                </Grid>
            </Grid>
        </>
    );

    function saveSettings() {
        const fixedPartition = partition ? partition.trim() : "";
        localStorage.setItem("partition", fixedPartition);
        context.setPartition(fixedPartition);
        history('/index.html');
    }
}


export default Settings;