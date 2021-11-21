import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {createStyles, FormLabel, Grid, makeStyles, TextField} from "@material-ui/core";
import {AppContext} from "../App";

const useStyles = makeStyles(() =>
    createStyles({
        label: {}
    })
);

const Settings: FC<CommonProps> = (props: CommonProps): ReactElement => {
    const context = useContext(AppContext);
    const classes = useStyles();
    const [apiKey, setApiKey] = useState<string | null>(localStorage.getItem("apiKey"));

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true}>
                <Grid className={classes.label} md={2} sm={12}>
                    <FormLabel>API Key</FormLabel>
                </Grid>
                <Grid item md={10} sm={12}>
                    <TextField id="apiKey" fullWidth={true} type="password" variant="outlined" value={apiKey}
                               onChange={v => {
                                   setApiKey(v.target.value);
                                   localStorage.setItem("apiKey", v.target.value);
                               }}/>
                </Grid>
            </Grid>

        </>
    );
}


export default Settings;