import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {AppContext} from "../App";
import {DataGrid} from "@material-ui/data-grid";

interface AuditsCollection {
    data: Audit[]
}

interface Audit {
    id: number,
    attributes: {
        subject: string,
        object: string,
        action: string,
        dataPartition: string
    }
}

const Audits: FC<CommonProps> = (props: CommonProps): ReactElement => {
    props.setAllBookId(null);

    const context = useContext(AppContext);

    const [audits, setAudits] = useState<AuditsCollection | null>(null);

    const columns = [
        { field: 'id', headerName: 'Id', width: 70 },
        { field: 'subject', headerName: 'Subject', width: 130 },
        { field: 'action', headerName: 'Action', width: 600 },
        { field: 'object', headerName: 'Object', width: 130 },
    ];

    useEffect(() => {
        fetch(context.settings.auditEndpoint, {
            headers: {
                'Accept': 'application/vnd.api+json; dataPartition=' + props.partition
            }
        })
            .then((response) => {
                if (response.ok) {
                    return response.json();
                } else {
                    throw new Error('Something went wrong');
                }
            })
            .then(data => setAudits(data));
    }, [setAudits, context.settings.auditEndpoint, props.partition]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            {!audits && <div>Loading...</div>}
            {audits && <DataGrid
                rows={audits.data.map((a: Audit) => ({id: a.id, subject: a.attributes.subject, action: a.attributes.action, object: a.attributes.object}))}
                columns={columns}
                pageSize={5}
                rowsPerPageOptions={[5]}
            />}

        </>
    );
}

export default Audits;